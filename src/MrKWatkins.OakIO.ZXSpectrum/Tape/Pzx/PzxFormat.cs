using System.Text;
using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The PZX tape image file format.
/// </summary>
// https://github.com/raxoft/pzxtools/blob/master/docs/pzx_format.txt
public sealed class PzxFormat : ZXSpectrumTapeFormat<PzxFile>
{
    /// <summary>
    /// The singleton instance of the PZX format.
    /// </summary>
    public static readonly PzxFormat Instance = new();

    private PzxFormat()
        : base("PZX Tape", "pzx")
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        var pzxToTape = new PzxToTapeConverter();
        yield return new PzxToTzxConverter();
        yield return new PzxToTapConverter();
        yield return pzxToTape;
        yield return new WavFileViaTapeConverter<PzxFile>(Instance, pzxToTape, new TapeToWavConverter(TStatesPerSecond));
    }

    /// <inheritdoc />
    protected override PzxFile ReadTape(Stream stream)
    {
        var blocks = new List<PzxBlock>();
        blocks.AddRange(ReadBlocks(stream));

        return new PzxFile(blocks);
    }

    private static IEnumerable<PzxBlock> ReadBlocks(Stream stream)
    {
        using var peekable = new PeekableStream(stream);
        while (true)
        {
            if (peekable.Peek() == -1)
            {
                yield break;
            }

            var typeBytes = peekable.ReadExactly(4);
            var type = (PzxBlockType)typeBytes.GetUInt32(0, Endian.Big);
            yield return type switch
            {
                PzxBlockType.Header => new PzxHeaderBlock(peekable),
                PzxBlockType.PulseSequence => new PulseSequenceBlock(peekable),
                PzxBlockType.Data => new DataBlock(peekable),
                PzxBlockType.Pause => new PauseBlock(peekable),
                PzxBlockType.BrowsePoint => new BrowsePointBlock(peekable),
                PzxBlockType.Stop => new StopBlock(peekable),
                _ => throw new NotSupportedException($"The block type {Encoding.ASCII.GetString(typeBytes)} is not supported.")
            };
        }
    }

    /// <inheritdoc />
    protected override void Write(PzxFile file, Stream stream)
    {
        Span<byte> tagBytes = stackalloc byte[4];
        foreach (var block in file.Blocks)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(tagBytes, (uint)block.Header.Type);
            stream.Write(tagBytes);
            block.Header.Write(stream);
            block.Write(stream);
        }
    }
}