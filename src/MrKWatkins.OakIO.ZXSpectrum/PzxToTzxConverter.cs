using System.Text;
using MrKWatkins.BinaryPrimitives;
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
            blocks.AddRange(ConvertBlock(pzxBlock));
        }

        return new TzxFile(header, blocks);
    }

    private static IEnumerable<TzxBlock> ConvertBlock(PzxBlock pzxBlock) =>
        pzxBlock switch
        {
            PzxHeaderBlock headerBlock => ConvertHeaderBlock(headerBlock),
            PzxPulseSequenceBlock pulseBlock => ConvertPulseSequenceBlock(pulseBlock),
            DataBlock dataBlock => ConvertDataBlock(dataBlock),
            Pzx.PauseBlock pauseBlock => ConvertPauseBlock(pauseBlock),
            StopBlock stopBlock => ConvertStopBlock(stopBlock),
            BrowsePointBlock browseBlock => ConvertBrowsePointBlock(browseBlock),
            _ => [],
        };

    private static IEnumerable<TzxBlock> ConvertHeaderBlock(PzxHeaderBlock block)
    {
        var entries = block.Info
            .Select(info => (Type: MapInfoType(info.Type), Bytes: Encoding.ASCII.GetBytes(info.Text)))
            .Where(e => e.Bytes.Length <= 255)
            .ToList();

        if (entries.Count == 0)
        {
            yield break;
        }

        using var entriesStream = new MemoryStream();
        foreach (var (type, bytes) in entries)
        {
            entriesStream.WriteByte((byte)type);
            entriesStream.WriteByte((byte)bytes.Length);
            entriesStream.Write(bytes);
        }

        var entriesData = entriesStream.ToArray();
        var wholeBlockLength = (ushort)(entriesData.Length + 1);

        using var stream = new MemoryStream();
        stream.WriteWord(wholeBlockLength);
        stream.WriteByte((byte)entries.Count);
        stream.Write(entriesData);
        stream.Position = 0;
        yield return new ArchiveInfoBlock(stream);
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

    private static IEnumerable<TzxBlock> ConvertPulseSequenceBlock(PzxPulseSequenceBlock block)
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
                foreach (var flushed in FlushSinglePulses(singlePulses))
                {
                    yield return flushed;
                }

                using var stream = new MemoryStream();
                stream.WriteWord((ushort)Math.Min(pulse.Duration, ushort.MaxValue));
                stream.WriteWord(pulse.Count);
                stream.Position = 0;
                yield return new PureToneBlock(stream);
            }
            else
            {
                singlePulses.Add((ushort)Math.Min(pulse.Duration, ushort.MaxValue));
            }
        }

        foreach (var flushed in FlushSinglePulses(singlePulses))
        {
            yield return flushed;
        }
    }

    private static IEnumerable<TzxBlock> FlushSinglePulses(List<ushort> pulses)
    {
        foreach (var chunk in pulses.Chunk(255))
        {
            using var stream = new MemoryStream();
            stream.WriteByte((byte)chunk.Length);
            foreach (var pulse in chunk)
            {
                stream.WriteWord(pulse);
            }

            stream.Position = 0;
            yield return new TzxPulseSequenceBlock(stream);
        }

        pulses.Clear();
    }

    private static IEnumerable<TzxBlock> ConvertDataBlock(DataBlock block)
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
        stream.WriteWord(zeroBitTStates);
        stream.WriteWord(oneBitTStates);
        stream.WriteByte(usedBitsInLastByte);
        stream.WriteWord(0);
        stream.WriteUInt24(dataStream.Length);
        stream.Write(dataStream);
        stream.Position = 0;
        yield return new PureDataBlock(stream);
    }

    private static IEnumerable<TzxBlock> ConvertPauseBlock(Pzx.PauseBlock block)
    {
        var durationMs = (ushort)Math.Min(block.Header.Duration / MillisecondCycles, ushort.MaxValue);

        using var stream = new MemoryStream();
        stream.WriteWord(durationMs);
        stream.Position = 0;
        yield return new TzxPauseBlock(stream);
    }

    private static IEnumerable<TzxBlock> ConvertStopBlock(StopBlock block)
    {
        if (block.Header.Only48k)
        {
            using var stream = new MemoryStream([0x00, 0x00, 0x00, 0x00]);
            yield return new StopTheTapeIf48KBlock(stream);
        }
        else
        {
            using var stream = new MemoryStream([0x00, 0x00]);
            yield return new TzxPauseBlock(stream);
        }
    }

    private static IEnumerable<TzxBlock> ConvertBrowsePointBlock(BrowsePointBlock block)
    {
        var textBytes = Encoding.ASCII.GetBytes(block.Text);
        var length = Math.Min(textBytes.Length, 255);

        using var stream = new MemoryStream();
        stream.WriteByte((byte)length);
        stream.Write(textBytes, 0, length);
        stream.Position = 0;
        yield return new TextDescriptionBlock(stream);
    }
}
