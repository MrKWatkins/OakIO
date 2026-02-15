using System.Text;
using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

// TODO: Remove logging.
// https://github.com/raxoft/pzxtools/blob/master/docs/pzx_format.txt
public sealed class PzxFormat : TapeFormat<PzxFile>
{
    public static readonly PzxFormat Instance = new();

    private PzxFormat()
        : base("PZX Tape", "pzx")
    {
    }

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

#pragma warning disable CS0219
            // TODO: Using MrKWatkins.BinaryPrimitives.
            var typeBytes = peekable.ReadExactly(4);
            var type = (PzxBlockType)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(typeBytes);
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
#pragma warning restore CS0219
        }
    }

    protected override void Write(PzxFile file, Stream stream)
    {
        throw new NotImplementedException();
    }
}