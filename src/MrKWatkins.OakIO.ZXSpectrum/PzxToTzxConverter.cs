using System.Text;
using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;
using PzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.PulseSequenceBlock;
using TzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Tzx.PulseSequenceBlock;
using TzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Tzx.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum;

/// <summary>
/// Converts PZX tape files to TZX format.
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
            _ => []
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

        var entriesSize = entries.Sum(e => 2 + e.Bytes.Length);
        var header = new byte[3];
        header.SetWord(0, (ushort)(entriesSize + 1));
        header[2] = (byte)entries.Count;

        var body = new byte[entriesSize];
        var offset = 0;
        foreach (var (type, bytes) in entries)
        {
            body[offset++] = (byte)type;
            body[offset++] = (byte)bytes.Length;
            Array.Copy(bytes, 0, body, offset, bytes.Length);
            offset += bytes.Length;
        }

        yield return new ArchiveInfoBlock(header, body);
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
            _ => throw new NotSupportedException($"The {nameof(ArchiveInfoType)} {type} is not supported.")
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

            if (pulse.Duration > ushort.MaxValue)
            {
                foreach (var flushed in FlushSinglePulses(singlePulses))
                {
                    yield return flushed;
                }

                var splitPulses = SplitDuration(pulse.Duration).ToArray();
                for (var i = 0; i < pulse.Count; i++)
                {
                    singlePulses.AddRange(splitPulses);
                }
            }
            else if (pulse.Count > 1)
            {
                foreach (var flushed in FlushSinglePulses(singlePulses))
                {
                    yield return flushed;
                }

                var header = new byte[4];
                header.SetWord(0, (ushort)pulse.Duration);
                header.SetWord(2, pulse.Count);
                yield return new PureToneBlock(header);
            }
            else
            {
                singlePulses.Add((ushort)pulse.Duration);
            }
        }

        foreach (var flushed in FlushSinglePulses(singlePulses))
        {
            yield return flushed;
        }
    }

    private static IEnumerable<ushort> SplitDuration(uint duration)
    {
        while (duration > ushort.MaxValue)
        {
            yield return ushort.MaxValue;
            duration -= ushort.MaxValue;
        }

        if (duration > 0)
        {
            yield return (ushort)duration;
        }
    }

    private static IEnumerable<TzxBlock> FlushSinglePulses(List<ushort> pulses)
    {
        foreach (var chunk in pulses.Chunk(255))
        {
            var header = new byte[1];
            header[0] = (byte)chunk.Length;

            var body = new byte[chunk.Length * 2];
            var offset = 0;
            foreach (var pulse in chunk)
            {
                body.SetWord(offset, pulse);
                offset += 2;
            }

            yield return new TzxPulseSequenceBlock(header, body);
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

        var header = new byte[10];
        header.SetWord(0, zeroBitTStates);
        header.SetWord(2, oneBitTStates);
        header[4] = usedBitsInLastByte;
        header.SetWord(5, 0);
        header.SetUInt24(7, dataStream.Length);

        yield return new PureDataBlock(header, dataStream.ToArray());
    }

    private static IEnumerable<TzxBlock> ConvertPauseBlock(Pzx.PauseBlock block)
    {
        var durationMs = (ushort)Math.Min(block.Header.Duration / MillisecondCycles, ushort.MaxValue);

        var header = new byte[2];
        header.SetWord(0, durationMs);
        yield return new TzxPauseBlock(header);
    }

    private static IEnumerable<TzxBlock> ConvertStopBlock(StopBlock block)
    {
        if (block.Header.Only48k)
        {
            yield return new StopTheTapeIf48KBlock([0x00, 0x00, 0x00, 0x00]);
        }
        else
        {
            yield return new TzxPauseBlock([0x00, 0x00]);
        }
    }

    private static IEnumerable<TzxBlock> ConvertBrowsePointBlock(BrowsePointBlock block)
    {
        var textBytes = Encoding.ASCII.GetBytes(block.Text);
        var length = Math.Min(textBytes.Length, 255);

        var header = new byte[1];
        header[0] = (byte)length;

        yield return new TextDescriptionBlock(header, textBytes[..length]);
    }
}