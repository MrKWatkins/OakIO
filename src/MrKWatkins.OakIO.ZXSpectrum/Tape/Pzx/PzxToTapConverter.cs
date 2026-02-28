using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// Converts PZX files to TAP format. Only data blocks can be converted; other block types
/// such as pulse sequences, headers, pauses and browse points are skipped.
/// </summary>
public sealed class PzxToTapConverter : IOFileConverter<PzxFile, TapFile>
{
    internal PzxToTapConverter()
        : base(PzxFormat.Instance, TapFormat.Instance)
    {
    }

    /// <inheritdoc />
    [Pure]
    public override TapFile Convert(PzxFile source)
    {
        var blocks = new List<TapBlock>();

        foreach (var block in source.Blocks)
        {
            switch (block)
            {
                case DataBlock data:
                    blocks.Add(ConvertBlock(data));
                    break;

                // Metadata and structural blocks can be safely skipped.
                case PzxHeaderBlock:
                case PauseBlock:
                case BrowsePointBlock:
                case StopBlock:
                case PulseSequenceBlock:
                    break;

                default:
                    throw new NotSupportedException($"Cannot convert PZX to TAP: the {block.Header.Type} block type cannot be represented in a TAP file.");
            }
        }

        if (blocks.Count == 0)
        {
            throw new NotSupportedException("Cannot convert PZX to TAP: the file contains no data blocks.");
        }

        return new TapFile(blocks);
    }

    [Pure]
    private static TapBlock ConvertBlock(DataBlock block)
    {
        var data = block.DataStream;
        var flag = data[0];
        var bodyData = data[1..^1].ToArray();
        var checksum = data[^1];
        var blockLength = (ushort)data.Length;

        if (flag == (byte)TapBlockType.Header && blockLength == 19)
        {
            return new HeaderBlock(new HeaderHeader(blockLength), new TapTrailer(checksum), bodyData);
        }

        return new Tap.DataBlock(new Tap.DataHeader(blockLength), new TapTrailer(checksum), bodyData);
    }
}