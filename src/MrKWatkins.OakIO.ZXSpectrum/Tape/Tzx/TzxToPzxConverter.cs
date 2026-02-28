using System.Text;
using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using PzxDataBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.DataBlock;
using PzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.PauseBlock;
using PzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.PulseSequenceBlock;
using TzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx.PauseBlock;
using TzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx.PulseSequenceBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Converts TZX tape files to PZX format.
/// </summary>
public sealed class TzxToPzxConverter : IOFileConverter<TzxFile, PzxFile>
{
    private const ushort LeaderCycles = 2168;
    private const ushort ShortLeaderCount = 3223;
    private const ushort LongLeaderCount = 8063;
    private const ushort Sync1Cycles = 667;
    private const ushort Sync2Cycles = 735;
    private const ushort Bit0Cycles = 855;
    private const ushort Bit1Cycles = 1710;
    private const ushort TailCycles = 945;
    private const ushort MillisecondCycles = 3500;

    private const byte PzxMajorVersion = 1;
    private const byte PzxMinorVersion = 0;

    // PZX format limits.
    private const uint MaxPulseDuration = 0x7FFFFFFF;
    private const ushort MaxRepeatCount = 0x7FFF;

    // Maximum nesting level for loop/call blocks to prevent infinite recursion.
    private const int MaxNestingLevel = 10;

    internal TzxToPzxConverter()
        : base(TzxFormat.Instance, PzxFormat.Instance)
    {
    }

    /// <inheritdoc />
    [Pure]
    public override PzxFile Convert(TzxFile source)
    {
        using var context = new ConversionContext(source);
        ProcessBlocks(context, 0, null, 0);
        context.FlushPulses();

        var blocks = new List<PzxBlock> { BuildHeaderBlock(context.InfoEntries) };
        blocks.AddRange(context.OutputBlocks);

        return new PzxFile(blocks);
    }

    private static int ProcessBlocks(ConversionContext context, int index, TzxBlockType? endType, int nestingLevel)
    {
        if (nestingLevel > MaxNestingLevel)
        {
            return index;
        }

        while (index < context.Blocks.Count)
        {
            var block = context.Blocks[index];
            index++;

            if (endType == TzxBlockType.LoopEnd && block is LoopEndBlock)
            {
                return index;
            }

            switch (block)
            {
                case StandardSpeedDataBlock stdBlock:
                    ConvertStandardSpeedData(context, stdBlock);
                    break;
                case TurboSpeedDataBlock turboBlock:
                    ConvertTurboSpeedData(context, turboBlock);
                    break;
                case PureToneBlock toneBlock:
                    ConvertPureTone(context, toneBlock);
                    break;
                case TzxPulseSequenceBlock pulseBlock:
                    ConvertPulseSequence(context, pulseBlock);
                    break;
                case PureDataBlock dataBlock:
                    ConvertPureData(context, dataBlock);
                    break;
                case TzxPauseBlock pauseBlock:
                    ConvertPause(context, pauseBlock);
                    break;
                case StopTheTapeIf48KBlock:
                    context.EmitStopBlock(1);
                    break;
                case GroupStartBlock groupStart:
                    context.EmitBrowseBlock(groupStart.Text);
                    break;
                case GroupEndBlock:
                    break;
                case TextDescriptionBlock textBlock:
                    context.EmitBrowseBlock(textBlock.Text);
                    break;
                case ArchiveInfoBlock archiveBlock:
                    ConvertArchiveInfo(context, archiveBlock);
                    break;
                case LoopStartBlock loopBlock:
                    {
                        var bodyStart = index;
                        for (var i = 0; i < loopBlock.Header.NumberOfRepetitions; i++)
                        {
                            index = ProcessBlocks(context, bodyStart, TzxBlockType.LoopEnd, nestingLevel + 1);
                        }
                        break;
                    }
                case LoopEndBlock:
                    break;
            }
        }

        return index;
    }

    private static void ConvertStandardSpeedData(ConversionContext context, StandardSpeedDataBlock block)
    {
        var data = block.Data;
        var leaderCount = data.Count > 0 && data[0] < 128 ? LongLeaderCount : ShortLeaderCount;
        RenderPilot(context, leaderCount, LeaderCycles, Sync1Cycles, Sync2Cycles);
        RenderData(context, data, data.Count, 8, Bit0Cycles, Bit1Cycles, TailCycles, block.Header.PauseAfterBlockMs);
    }

    private static void ConvertTurboSpeedData(ConversionContext context, TurboSpeedDataBlock block)
    {
        var header = block.Header;
        RenderPilot(context, header.PulsesInPilotTone, header.TStatesInPilotPulse, header.TStatesInSyncFirstPulse, header.TStatesInSyncSecondPulse);
        RenderData(context, block.Data, block.Data.Count, header.UsedBitsInLastByte,
            header.TStatesInZeroBitPulse, header.TStatesInOneBitPulse, 0, header.PauseAfterBlockMs);
    }

    private static void ConvertPureTone(ConversionContext context, PureToneBlock block)
    {
        RenderPulses(context, block.Header.NumberOfPulses, block.Header.LengthOfPulse);
    }

    private static void ConvertPulseSequence(ConversionContext context, TzxPulseSequenceBlock block)
    {
        foreach (var pulse in block.Pulses)
        {
            RenderPulse(context, pulse);
        }
    }

    private static void ConvertPureData(ConversionContext context, PureDataBlock block)
    {
        var header = block.Header;
        RenderData(context, block.Data, block.Data.Count, header.UsedBitsInLastByte,
            header.TStatesInZeroBitPulse, header.TStatesInOneBitPulse, 0, header.PauseAfterBlockMs);
    }

    private static void ConvertPause(ConversionContext context, TzxPauseBlock block)
    {
        var durationMs = block.Header.PauseMs;
        if (durationMs > 0)
        {
            RenderPause(context, durationMs);
        }
        else
        {
            context.EmitStopBlock(0);
        }
    }

    private static void ConvertArchiveInfo(ConversionContext context, ArchiveInfoBlock block)
    {
        var title = block.Entries.FirstOrDefault(e => e.Type == ArchiveInfoType.FullTitle)?.Text ?? "Some tape";
        context.AddInfo(title);

        foreach (var entry in block.Entries.Where(e => e.Type != ArchiveInfoType.FullTitle))
        {
            context.AddInfo(GetInfoTypeName(entry.Type));
            context.AddInfo(entry.Text);
        }
    }

    [Pure]
    private static string GetInfoTypeName(ArchiveInfoType type) =>
        type switch
        {
            ArchiveInfoType.SoftwareHouseOrPublisher => "Publisher",
            ArchiveInfoType.Authors => "Author",
            ArchiveInfoType.YearOfPublication => "Year",
            ArchiveInfoType.Language => "Language",
            ArchiveInfoType.GameOrUtilityType => "Type",
            ArchiveInfoType.Price => "Price",
            ArchiveInfoType.ProtectionSchemeOrLoader => "Protection",
            ArchiveInfoType.Origin => "Origin",
            ArchiveInfoType.Comments => "Comment",
            _ => "Info"
        };

    private static void RenderPulse(ConversionContext context, uint duration)
    {
        context.PzxOut(duration, context.Level);
        context.Level = !context.Level;
    }

    private static void RenderPulses(ConversionContext context, int count, uint duration)
    {
        for (var i = 0; i < count; i++)
        {
            RenderPulse(context, duration);
        }
    }

    private static void RenderPilot(ConversionContext context, int leaderCount, ushort leaderCycles, ushort sync1, ushort sync2)
    {
        RenderPulses(context, leaderCount, leaderCycles);
        RenderPulse(context, sync1);
        RenderPulse(context, sync2);
    }

    private static void RenderData(ConversionContext context, IReadOnlyList<byte> data, int dataSize, int bitsInLastByte,
        ushort bit0Cycles, ushort bit1Cycles, ushort tailCycles, ushort pauseMs)
    {
        if (bitsInLastByte == 0 && dataSize > 0)
        {
            bitsInLastByte = 8;
        }

        var bitCount = (uint)(8 * dataSize);
        if (bitsInLastByte < 8 && bitCount >= 8)
        {
            bitCount -= 8;
            bitCount += (uint)bitsInLastByte;
        }

        var s0 = new[] { bit0Cycles, bit0Cycles };
        var s1 = new[] { bit1Cycles, bit1Cycles };

        RenderDataCore(context, context.Level, context.Level, context.Level, data, bitCount, 2, 2, s0, s1, tailCycles, pauseMs);
    }

    private static void RenderDataCore(ConversionContext context, bool initialLevel, bool finalLevel0, bool finalLevel1,
        IReadOnlyList<byte> data, uint bitCount, byte numZero, byte numOne, ushort[] s0, ushort[] s1, ushort tailCycles, ushort pauseMs)
    {
        if (bitCount > 0)
        {
            var dataByteCount = (int)((bitCount + 7) / 8);
            var dataBytes = data.Take(dataByteCount).ToArray();

            context.EmitDataBlock(dataBytes, bitCount, initialLevel, numZero, numOne, s0, s1, tailCycles);

            var bitIndex = (int)(bitCount - 1);
            var bitMask = 0x80 >> (bitIndex & 7);
            var lastByte = dataBytes[bitIndex / 8];
            context.Level = (lastByte & bitMask) != 0 ? finalLevel1 : finalLevel0;
        }

        if (pauseMs > 0)
        {
            context.Level = false;
            context.EmitPauseBlock((uint)pauseMs * MillisecondCycles, context.Level);
        }
    }

    private static void RenderPause(ConversionContext context, ushort durationMs)
    {
        if (durationMs == 0)
        {
            return;
        }

        if (context.Level)
        {
            RenderPulse(context, MillisecondCycles);
        }

        context.EmitPauseBlock((uint)durationMs * MillisecondCycles, context.Level);
    }

    [Pure]
    private static PzxHeaderBlock BuildHeaderBlock(List<string> infoStrings)
    {
        var infoBytes = Encoding.ASCII.GetBytes(string.Join('\0', infoStrings));

        var header = new byte[6];
        header.SetUInt32(0, (uint)(2 + infoBytes.Length));
        header[4] = PzxMajorVersion;
        header[5] = PzxMinorVersion;

        return new PzxHeaderBlock(header, infoBytes);
    }

    private sealed class ConversionContext(TzxFile source) : IDisposable
    {
        private uint lastDuration;
        private bool lastLevel;
        private uint pendingPulseCount;
        private uint pendingPulseDuration;
        private readonly MemoryStream pulseBuffer = new();

        public void Dispose() => pulseBuffer.Dispose();

        public IReadOnlyList<TzxBlock> Blocks { get; } = source.Blocks;
        public List<PzxBlock> OutputBlocks { get; } = [];
        public List<string> InfoEntries { get; } = [];
        public bool Level { get; set; }

        public void AddInfo(string text) => InfoEntries.Add(text);

        /// <summary>
        /// Outputs a pulse of the given duration at the given level, accumulating consecutive same-level durations.
        /// </summary>
        public void PzxOut(uint duration, bool level)
        {
            if (duration == 0)
            {
                return;
            }

            if (duration > MaxPulseDuration)
            {
                PzxOut(MaxPulseDuration, level);
                PzxOut(duration - MaxPulseDuration, level);
                return;
            }

            if (lastLevel != level)
            {
                StorePulse(lastDuration);
                lastDuration = 0;
                lastLevel = level;
            }

            lastDuration += duration;

            if (lastDuration > MaxPulseDuration)
            {
                StorePulse(MaxPulseDuration);
                StorePulse(0);
                lastDuration -= MaxPulseDuration;
            }
        }

        /// <summary>
        /// Stores a pulse of the given duration, compressing runs of identical durations.
        /// </summary>
        private void StorePulse(uint duration)
        {
            if (pendingPulseCount > 0)
            {
                if (pendingPulseDuration == duration && pendingPulseCount < MaxRepeatCount)
                {
                    pendingPulseCount++;
                    return;
                }
                EncodePulse(pendingPulseCount, pendingPulseDuration);
            }
            pendingPulseDuration = duration;
            pendingPulseCount = 1;
        }

        /// <summary>
        /// Encodes a pulse with the given count and duration to the pulse buffer.
        /// </summary>
        private void EncodePulse(uint count, uint duration)
        {
            if (count > 1 || duration > 0xFFFF)
            {
                pulseBuffer.WriteUInt16((ushort)(0x8000 | count));
            }

            if (duration < 0x8000)
            {
                pulseBuffer.WriteUInt16((ushort)duration);
            }
            else
            {
                pulseBuffer.WriteUInt16((ushort)(0x8000 | (duration >> 16)));
                pulseBuffer.WriteUInt16((ushort)(duration & 0xFFFF));
            }
        }

        /// <summary>
        /// Flushes accumulated pulses to output as a pulse sequence block.
        /// </summary>
        public void FlushPulses()
        {
            if (lastDuration > 0)
            {
                StorePulse(lastDuration);
                lastDuration = 0;
                lastLevel = false;
            }

            if (pendingPulseCount > 0)
            {
                EncodePulse(pendingPulseCount, pendingPulseDuration);
                pendingPulseCount = 0;
            }

            if (pulseBuffer.Length > 0)
            {
                EmitPulseSequenceBlock();
            }
        }

        private void EmitPulseSequenceBlock()
        {
            var pulseData = pulseBuffer.ToArray();
            pulseBuffer.SetLength(0);

            var header = new byte[4];
            header.SetUInt32(0, (uint)pulseData.Length);

            OutputBlocks.Add(new PzxPulseSequenceBlock(header, pulseData));
        }

        public void EmitDataBlock(byte[] data, uint bitCount, bool initialLevel, byte numZero, byte numOne, ushort[] zeroPulseSeq, ushort[] onePulseSeq, ushort tailCycles)
        {
            FlushPulses();

            var dataByteCount = (int)((bitCount + 7) / 8);

            var header = new byte[12];
            header.SetUInt32(0, (uint)(8 + numZero * 2 + numOne * 2 + dataByteCount));
            header.SetUInt32(4, (initialLevel ? 0x80000000u : 0) | bitCount);
            header.SetUInt16(8, tailCycles);
            header[10] = numZero;
            header[11] = numOne;

            var bodySize = numZero * 2 + numOne * 2 + dataByteCount;
            var body = new byte[bodySize];
            var offset = 0;
            foreach (var p in zeroPulseSeq)
            {
                body.SetUInt16(offset, p);
                offset += 2;
            }
            foreach (var p in onePulseSeq)
            {
                body.SetUInt16(offset, p);
                offset += 2;
            }
            Array.Copy(data, 0, body, offset, dataByteCount);

            OutputBlocks.Add(new PzxDataBlock(header, body));
        }

        public void EmitPauseBlock(uint durationTStates, bool level)
        {
            FlushPulses();

            var header = new byte[8];
            header.SetUInt32(0, 4);
            header.SetUInt32(4, (level ? 0x80000000u : 0) | (durationTStates & 0x7FFFFFFF));

            OutputBlocks.Add(new PzxPauseBlock(header));
        }

        public void EmitStopBlock(ushort flags)
        {
            FlushPulses();

            var header = new byte[6];
            header.SetUInt32(0, 2);
            header.SetUInt16(4, flags);

            OutputBlocks.Add(new StopBlock(header));
        }

        public void EmitBrowseBlock(string text)
        {
            FlushPulses();

            var textBytes = Encoding.ASCII.GetBytes(text);

            var header = new byte[4];
            header.SetUInt32(0, (uint)textBytes.Length);

            OutputBlocks.Add(new BrowsePointBlock(header, textBytes));
        }
    }
}