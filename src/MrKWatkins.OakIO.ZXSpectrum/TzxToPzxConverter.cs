using System.Text;
using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;
using PzxDataBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.DataBlock;
using PzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.PauseBlock;
using PzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.PulseSequenceBlock;
using TzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Tzx.PauseBlock;
using TzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Tzx.PulseSequenceBlock;

namespace MrKWatkins.OakIO.ZXSpectrum;

/// <summary>
/// Converts TZX tape files to PZX format. Based on the conversion logic defined in the pzxtools reference implementation (tzx.cpp + pzx.cpp).
/// </summary>
public sealed class TzxToPzxConverter : IFormatConverter<TzxFile, PzxFile>
{
    // Standard ZX Spectrum ROM loader constants (from tap.h).
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

    // Maximum nesting level for loop/call blocks to prevent infinite recursion (matches pzxtools reference).
    private const int MaxNestingLevel = 10;

    [Pure]
    public PzxFile Convert(TzxFile source)
    {
        using var context = new ConversionContext();
        var index = 0;
        ProcessBlocks(source.Blocks, ref index, context, null, 0);
        context.FlushPulses();

        var blocks = new List<PzxBlock>();
        blocks.Add(BuildHeaderBlock(context.InfoEntries));
        blocks.AddRange(context.OutputBlocks);

        return new PzxFile(blocks);
    }

    private static void ProcessBlocks(IReadOnlyList<TzxBlock> blocks, ref int index, ConversionContext context, TzxBlockType? endType, int nestingLevel)
    {
        if (nestingLevel > MaxNestingLevel)
        {
            return;
        }

        while (index < blocks.Count)
        {
            var block = blocks[index];
            index++;

            if (endType == TzxBlockType.LoopEnd && block is LoopEndBlock)
            {
                return;
            }

            switch (block)
            {
                case StandardSpeedDataBlock stdBlock:
                    ConvertStandardSpeedData(stdBlock, context);
                    break;
                case TurboSpeedDataBlock turboBlock:
                    ConvertTurboSpeedData(turboBlock, context);
                    break;
                case PureToneBlock toneBlock:
                    ConvertPureTone(toneBlock, context);
                    break;
                case TzxPulseSequenceBlock pulseBlock:
                    ConvertPulseSequence(pulseBlock, context);
                    break;
                case PureDataBlock dataBlock:
                    ConvertPureData(dataBlock, context);
                    break;
                case TzxPauseBlock pauseBlock:
                    ConvertPause(pauseBlock, context);
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
                    ConvertArchiveInfo(archiveBlock, context);
                    break;
                case LoopStartBlock loopBlock:
                {
                    var bodyStart = index;
                    for (var i = 0; i < loopBlock.Header.NumberOfRepetitions; i++)
                    {
                        index = bodyStart;
                        ProcessBlocks(blocks, ref index, context, TzxBlockType.LoopEnd, nestingLevel + 1);
                    }
                    break;
                }
                case LoopEndBlock:
                    break;
            }
        }
    }

    private static void ConvertStandardSpeedData(StandardSpeedDataBlock block, ConversionContext context)
    {
        var data = block.Data;
        var leaderCount = data.Count > 0 && data[0] < 128 ? LongLeaderCount : ShortLeaderCount;
        RenderPilot(context, leaderCount, LeaderCycles, Sync1Cycles, Sync2Cycles);
        RenderData(context, data, data.Count, 8, Bit0Cycles, Bit1Cycles, TailCycles, block.Header.PauseAfterBlockMs);
    }

    private static void ConvertTurboSpeedData(TurboSpeedDataBlock block, ConversionContext context)
    {
        var header = block.Header;
        RenderPilot(context, header.PulsesInPilotTone, header.TStatesInPilotPulse, header.TStatesInSyncFirstPulse, header.TStatesInSyncSecondPulse);
        RenderData(context, block.Data, block.Data.Count, header.UsedBitsInLastByte,
            header.TStatesInZeroBitPulse, header.TStatesInOneBitPulse, TailCycles, header.PauseAfterBlockMs);
    }

    private static void ConvertPureTone(PureToneBlock block, ConversionContext context)
    {
        RenderPulses(context, block.Header.NumberOfPulses, block.Header.LengthOfPulse);
    }

    private static void ConvertPulseSequence(TzxPulseSequenceBlock block, ConversionContext context)
    {
        var pulses = block.Pulses;
        for (var i = 0; i < pulses.Length; i++)
        {
            RenderPulse(context, pulses[i]);
        }
    }

    private static void ConvertPureData(PureDataBlock block, ConversionContext context)
    {
        var header = block.Header;
        RenderData(context, block.Data, block.Data.Count, header.UsedBitsInLastByte,
            header.TStatesInZeroBitPulse, header.TStatesInOneBitPulse, TailCycles, header.PauseAfterBlockMs);
    }

    private static void ConvertPause(TzxPauseBlock block, ConversionContext context)
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

    private static void ConvertArchiveInfo(ArchiveInfoBlock block, ConversionContext context)
    {
        // First pass: extract title (matching tzx_convert_info with title_only=true).
        string? title = null;
        foreach (var entry in block.Entries)
        {
            if (entry.Type == ArchiveInfoType.FullTitle)
            {
                title = entry.Text;
                break;
            }
        }
        // Default title used by the pzxtools reference implementation when no title is found.
        context.AddInfo(title ?? "Some tape");

        // Second pass: extract other entries (matching tzx_convert_info with title_only=false).
        foreach (var entry in block.Entries)
        {
            if (entry.Type == ArchiveInfoType.FullTitle)
            {
                continue;
            }

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
            _ => "Info",
        };

    // --- Rendering helpers (equivalent to tzx.cpp functions) ---

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
        // Adjust total bit count when the last byte is partial: remove 8 bits for the full byte, add actual used bits.
        var bitCount = (uint)(8 * dataSize);
        if (bitsInLastByte <= 8 && bitCount >= 8)
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
            var tail = pauseMs > 0 ? tailCycles : (ushort)0;
            var dataByteCount = (int)((bitCount + 7) / 8);
            var dataBytes = new byte[dataByteCount];
            for (var i = 0; i < dataByteCount && i < data.Count; i++)
            {
                dataBytes[i] = data[i];
            }

            context.EmitDataBlock(dataBytes, bitCount, initialLevel, numZero, numOne, s0, s1, tail);

            // Adjust level based on last bit output.
            var bitIndex = (int)(bitCount - 1);
            var bitMask = 0x80 >> (bitIndex & 7);
            var lastByte = dataBytes[bitIndex / 8];
            context.Level = ((lastByte & bitMask) != 0) ? finalLevel1 : finalLevel0;
        }

        // Emit separate pause block unless it's a 1ms pause with tail and data (the tail pulse serves as the pause).
        if (pauseMs > 0)        {
            context.Level = false;
            if (pauseMs > 1 || tailCycles == 0 || bitCount == 0)
            {
                context.EmitPauseBlock((uint)pauseMs * MillisecondCycles, context.Level);
            }
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

    // --- Header block construction ---

    [Pure]
    private static PzxHeaderBlock BuildHeaderBlock(List<string> infoStrings)
    {
        using var stream = new MemoryStream();

        var infoDataSize = 0;
        for (var i = 0; i < infoStrings.Count; i++)
        {
            if (i > 0)
            {
                infoDataSize++;
            }
            infoDataSize += Encoding.ASCII.GetByteCount(infoStrings[i]);
        }

        var size = (uint)(2 + infoDataSize);
        WriteUInt32LE(stream, size);
        stream.WriteByte(PzxMajorVersion);
        stream.WriteByte(PzxMinorVersion);

        for (var i = 0; i < infoStrings.Count; i++)
        {
            if (i > 0)
            {
                stream.WriteByte(0);
            }
            stream.Write(Encoding.ASCII.GetBytes(infoStrings[i]));
        }

        stream.Position = 0;
        return new PzxHeaderBlock(stream);
    }

    // --- Binary helpers ---

    private static void WriteWordLE(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
    }

    private static void WriteUInt32LE(Stream stream, uint value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
        stream.WriteByte((byte)((value >> 16) & 0xFF));
        stream.WriteByte((byte)((value >> 24) & 0xFF));
    }

    // --- Pulse accumulation context (mirrors pzx.cpp state machine) ---

    private sealed class ConversionContext : IDisposable
    {
        private uint _lastDuration;
        private bool _lastLevel;
        private uint _pendingPulseCount;
        private uint _pendingPulseDuration;
        private readonly MemoryStream _pulseBuffer = new();

        public void Dispose() => _pulseBuffer.Dispose();

        public List<PzxBlock> OutputBlocks { get; } = [];
        public List<string> InfoEntries { get; } = [];
        public bool Level { get; set; }

        public void AddInfo(string text) => InfoEntries.Add(text);

        /// <summary>
        /// Equivalent to pzx_out in pzx.cpp - output pulse of given duration at given level.
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

            if (_lastLevel != level)
            {
                StorePulse(_lastDuration);
                _lastDuration = 0;
                _lastLevel = level;
            }

            _lastDuration += duration;

            if (_lastDuration > MaxPulseDuration)
            {
                StorePulse(MaxPulseDuration);
                StorePulse(0);
                _lastDuration -= MaxPulseDuration;
            }
        }

        /// <summary>
        /// Equivalent to pzx_pulse in pzx.cpp - store pulse of given duration.
        /// </summary>
        private void StorePulse(uint duration)
        {
            if (_pendingPulseCount > 0)
            {
                if (_pendingPulseDuration == duration && _pendingPulseCount < MaxRepeatCount)
                {
                    _pendingPulseCount++;
                    return;
                }
                EncodePulse(_pendingPulseCount, _pendingPulseDuration);
            }
            _pendingPulseDuration = duration;
            _pendingPulseCount = 1;
        }

        /// <summary>
        /// Equivalent to pzx_store in pzx.cpp - encode pulse to PULS buffer.
        /// </summary>
        private void EncodePulse(uint count, uint duration)
        {
            if (count > 1 || duration > 0xFFFF)
            {
                WriteWordLE(_pulseBuffer, (ushort)(0x8000 | count));
            }

            if (duration < 0x8000)
            {
                WriteWordLE(_pulseBuffer, (ushort)duration);
            }
            else
            {
                WriteWordLE(_pulseBuffer, (ushort)(0x8000 | (duration >> 16)));
                WriteWordLE(_pulseBuffer, (ushort)(duration & 0xFFFF));
            }
        }

        /// <summary>
        /// Equivalent to pzx_flush in pzx.cpp - flush accumulated pulses to output.
        /// </summary>
        public void FlushPulses()
        {
            if (_lastDuration > 0)
            {
                StorePulse(_lastDuration);
                _lastDuration = 0;
                _lastLevel = false;
            }

            if (_pendingPulseCount > 0)
            {
                EncodePulse(_pendingPulseCount, _pendingPulseDuration);
                _pendingPulseCount = 0;
            }

            if (_pulseBuffer.Length > 0)
            {
                EmitPulseSequenceBlock();
            }
        }

        private void EmitPulseSequenceBlock()
        {
            var pulseData = _pulseBuffer.ToArray();
            _pulseBuffer.SetLength(0);

            using var stream = new MemoryStream();
            WriteUInt32LE(stream, (uint)pulseData.Length);
            stream.Write(pulseData);
            stream.Position = 0;
            OutputBlocks.Add(new PzxPulseSequenceBlock(stream));
        }

        public void EmitDataBlock(byte[] data, uint bitCount, bool initialLevel, byte numZero, byte numOne, ushort[] zeroPulseSeq, ushort[] onePulseSeq, ushort tailCycles)
        {
            FlushPulses();

            var dataByteCount = (int)((bitCount + 7) / 8);
            var size = (uint)(8 + numZero * 2 + numOne * 2 + dataByteCount);

            using var stream = new MemoryStream();
            WriteUInt32LE(stream, size);
            WriteUInt32LE(stream, (initialLevel ? 0x80000000u : 0) | bitCount);
            WriteWordLE(stream, tailCycles);
            stream.WriteByte(numZero);
            stream.WriteByte(numOne);
            foreach (var p in zeroPulseSeq)
            {
                WriteWordLE(stream, p);
            }
            foreach (var p in onePulseSeq)
            {
                WriteWordLE(stream, p);
            }
            stream.Write(data, 0, dataByteCount);
            stream.Position = 0;
            OutputBlocks.Add(new PzxDataBlock(stream));
        }

        public void EmitPauseBlock(uint durationTStates, bool level)
        {
            FlushPulses();

            using var stream = new MemoryStream();
            WriteUInt32LE(stream, 4);
            WriteUInt32LE(stream, (level ? 0x80000000u : 0) | (durationTStates & 0x7FFFFFFF));
            stream.Position = 0;
            OutputBlocks.Add(new PzxPauseBlock(stream));
        }

        public void EmitStopBlock(ushort flags)
        {
            FlushPulses();

            using var stream = new MemoryStream();
            WriteUInt32LE(stream, 2);
            WriteWordLE(stream, flags);
            stream.Position = 0;
            OutputBlocks.Add(new StopBlock(stream));
        }

        public void EmitBrowseBlock(string text)
        {
            FlushPulses();

            var textBytes = Encoding.ASCII.GetBytes(text);
            using var stream = new MemoryStream();
            WriteUInt32LE(stream, (uint)textBytes.Length);
            stream.Write(textBytes);
            stream.Position = 0;
            OutputBlocks.Add(new BrowsePointBlock(stream));
        }
    }
}
