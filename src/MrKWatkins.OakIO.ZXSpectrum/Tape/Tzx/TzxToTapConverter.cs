using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Converts TZX files to TAP format. Only standard speed data blocks can be converted; other data-carrying
/// block types will cause the conversion to fail with a <see cref="NotSupportedException" />.
/// </summary>
public sealed class TzxToTapConverter : IOFileConverter<TzxFile, TapFile>
{
    internal TzxToTapConverter()
        : base(TzxFormat.Instance, TapFormat.Instance)
    {
    }

    /// <inheritdoc />
    [Pure]
    public override TapFile Convert(TzxFile source)
    {
        var blocks = new List<TapBlock>();

        foreach (var block in source.Blocks)
        {
            switch (block)
            {
                case StandardSpeedDataBlock ssdb:
                    blocks.Add(ConvertBlock(ssdb));
                    break;

                // Metadata and structural blocks can be safely skipped.
                case ArchiveInfoBlock:
                case TextDescriptionBlock:
                case GroupStartBlock:
                case GroupEndBlock:
                case PauseBlock:
                case StopTheTapeIf48KBlock:
                    break;

                default:
                    throw new NotSupportedException($"Cannot convert TZX to TAP: the {block.Header.Type} block type cannot be represented in a TAP file.");
            }
        }

        if (blocks.Count == 0)
        {
            throw new NotSupportedException("Cannot convert TZX to TAP: the file contains no standard speed data blocks.");
        }

        return new TapFile(blocks);
    }

    [Pure]
    private static TapBlock ConvertBlock(StandardSpeedDataBlock block)
    {
        var data = block.AsReadOnlySpan();
        var flag = data[0];
        var bodyData = data[1..^1].ToArray();
        var checksum = data[^1];
        var blockLength = (ushort)data.Length;

        if (flag == (byte)TapBlockType.Header && blockLength == 19)
        {
            return new HeaderBlock(new HeaderHeader(blockLength), new TapTrailer(checksum), bodyData);
        }

        return new DataBlock(new DataHeader(blockLength), new TapTrailer(checksum), bodyData);
    }
}