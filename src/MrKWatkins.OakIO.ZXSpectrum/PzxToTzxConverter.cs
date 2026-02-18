using System.Text;
using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;
using PzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.PulseSequenceBlock;
using TzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Tzx.PulseSequenceBlock;
using TzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Tzx.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum;

/// <summary>
/// Converts PZX tape files to TZX format. Based on the mapping defined in the pzxtools reference implementation.
/// </summary>
public sealed class PzxToTzxConverter : IFormatConverter<PzxFile, TzxFile>
{
    private const byte TzxMajorVersion = 1;
    private const byte TzxMinorVersion = 20;
    private const ushort MillisecondCycles = 3500;

    [Pure]
    public TzxFile Convert(PzxFile source)
    {
        var header = new TzxHeader(TzxMajorVersion, TzxMinorVersion);
        var blocks = new List<TzxBlock>();

        foreach (var pzxBlock in source.Blocks)
        {
            switch (pzxBlock)
            {
                case PzxHeaderBlock headerBlock:
                    ConvertHeaderBlock(headerBlock, blocks);
                    break;
                case PzxPulseSequenceBlock pulseBlock:
                    ConvertPulseSequenceBlock(pulseBlock, blocks);
                    break;
                case DataBlock dataBlock:
                    ConvertDataBlock(dataBlock, blocks);
                    break;
                case Pzx.PauseBlock pauseBlock:
                    ConvertPauseBlock(pauseBlock, blocks);
                    break;
                case StopBlock stopBlock:
                    ConvertStopBlock(stopBlock, blocks);
                    break;
                case BrowsePointBlock browseBlock:
                    ConvertBrowsePointBlock(browseBlock, blocks);
                    break;
            }
        }

        return new TzxFile(header, blocks);
    }

    private static void ConvertHeaderBlock(PzxHeaderBlock block, List<TzxBlock> output)
    {
        if (block.Info.Count == 0)
        {
            return;
        }

        using var entriesStream = new MemoryStream();
        byte count = 0;
        foreach (var info in block.Info)
        {
            var type = MapInfoType(info.Type);
            var textBytes = Encoding.ASCII.GetBytes(info.Text);
            if (textBytes.Length > 255)
            {
                continue;
            }

            entriesStream.WriteByte((byte)type);
            entriesStream.WriteByte((byte)textBytes.Length);
            entriesStream.Write(textBytes);
            count++;
        }

        if (count == 0)
        {
            return;
        }

        var entriesData = entriesStream.ToArray();
        var wholeBlockLength = (ushort)(entriesData.Length + 1);

        using var stream = new MemoryStream();
        WriteWordLE(stream, wholeBlockLength);
        stream.WriteByte(count);
        stream.Write(entriesData);
        stream.Position = 0;
        output.Add(new ArchiveInfoBlock(stream));
    }

    [Pure]
    private static ArchiveInfoType MapInfoType(string type) =>
        type switch
        {
            "Title" => ArchiveInfoType.FullTitle,
            "Publisher" => ArchiveInfoType.SoftwareHouseOrPublisher,
            "Author" => ArchiveInfoType.Authors,
            "Year" => ArchiveInfoType.YearOfPublication,
            "Language" => ArchiveInfoType.Language,
            "Type" => ArchiveInfoType.GameOrUtilityType,
            "Price" => ArchiveInfoType.Price,
            "Protection" => ArchiveInfoType.ProtectionSchemeOrLoader,
            "Origin" => ArchiveInfoType.Origin,
            "Comment" => ArchiveInfoType.Comments,
            _ => ArchiveInfoType.Comments,
        };

    private static void ConvertPulseSequenceBlock(PzxPulseSequenceBlock block, List<TzxBlock> output)
    {
        var singlePulses = new List<ushort>();

        foreach (var pulse in block.Pulses)
        {
            if (pulse.Duration == 0)
            {
                continue;
            }

            if (pulse.Count > 1)
            {
                FlushSinglePulses(singlePulses, output);

                using var stream = new MemoryStream();
                WriteWordLE(stream, (ushort)Math.Min(pulse.Duration, ushort.MaxValue));
                WriteWordLE(stream, pulse.Count);
                stream.Position = 0;
                output.Add(new PureToneBlock(stream));
            }
            else
            {
                singlePulses.Add((ushort)Math.Min(pulse.Duration, ushort.MaxValue));
            }
        }

        FlushSinglePulses(singlePulses, output);
    }

    private static void FlushSinglePulses(List<ushort> pulses, List<TzxBlock> output)
    {
        var index = 0;
        while (index < pulses.Count)
        {
            var count = Math.Min(255, pulses.Count - index);

            using var stream = new MemoryStream();
            stream.WriteByte((byte)count);
            for (var i = 0; i < count; i++)
            {
                WriteWordLE(stream, pulses[index + i]);
            }

            stream.Position = 0;
            output.Add(new TzxPulseSequenceBlock(stream));
            index += count;
        }

        pulses.Clear();
    }

    private static void ConvertDataBlock(DataBlock block, List<TzxBlock> output)
    {
        var zeroBitSeq = block.ZeroBitPulseSequence;
        var oneBitSeq = block.OneBitPulseSequence;

        var zeroBitTStates = zeroBitSeq.Length > 0 ? zeroBitSeq[0] : (ushort)0;
        var oneBitTStates = oneBitSeq.Length > 0 ? oneBitSeq[0] : (ushort)0;

        var sizeInBits = block.Header.SizeInBits;
        var usedBitsInLastByte = (byte)(sizeInBits % 8);
        if (usedBitsInLastByte == 0 && sizeInBits > 0)
        {
            usedBitsInLastByte = 8;
        }

        var dataStream = block.DataStream;

        using var stream = new MemoryStream();
        WriteWordLE(stream, zeroBitTStates);
        WriteWordLE(stream, oneBitTStates);
        stream.WriteByte(usedBitsInLastByte);
        WriteWordLE(stream, 0);
        WriteUInt24LE(stream, dataStream.Length);
        stream.Write(dataStream);
        stream.Position = 0;
        output.Add(new PureDataBlock(stream));
    }

    private static void ConvertPauseBlock(Pzx.PauseBlock block, List<TzxBlock> output)
    {
        var durationMs = (ushort)Math.Min(block.Header.Duration / MillisecondCycles, ushort.MaxValue);

        using var stream = new MemoryStream();
        WriteWordLE(stream, durationMs);
        stream.Position = 0;
        output.Add(new TzxPauseBlock(stream));
    }

    private static void ConvertStopBlock(StopBlock block, List<TzxBlock> output)
    {
        if (block.Header.Only48k)
        {
            using var stream = new MemoryStream([0x00, 0x00, 0x00, 0x00]);
            output.Add(new StopTheTapeIf48KBlock(stream));
        }
        else
        {
            using var stream = new MemoryStream([0x00, 0x00]);
            output.Add(new TzxPauseBlock(stream));
        }
    }

    private static void ConvertBrowsePointBlock(BrowsePointBlock block, List<TzxBlock> output)
    {
        var textBytes = Encoding.ASCII.GetBytes(block.Text);
        var length = Math.Min(textBytes.Length, 255);

        using var stream = new MemoryStream();
        stream.WriteByte((byte)length);
        stream.Write(textBytes, 0, length);
        stream.Position = 0;
        output.Add(new TextDescriptionBlock(stream));
    }

    private static void WriteWordLE(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
    }

    private static void WriteUInt24LE(Stream stream, int value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
        stream.WriteByte((byte)((value >> 16) & 0xFF));
    }
}
