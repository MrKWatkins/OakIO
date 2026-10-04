using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb;

/// <summary>
/// A headerless collection of byte-backed tile-index blocks with externally supplied dimensions and index width.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/benbaker76/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c (write_blocks and get_block).
/// </remarks>
public sealed class NxbFile : TileBlocksFile
{
    /// <summary>
    /// Creates an NXB file without deduplication, reindexing or discarding attributes.
    /// </summary>
    /// <param name="blocks">The block-major entries.</param>
    /// <param name="format">The explicit output layout.</param>
    public NxbFile(TileBlocksData blocks, NxbFormat format) : base(format)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        if (blocks.Width != format.Width || blocks.Height != format.Height)
        {
            throw new ArgumentException("The block dimensions must match the selected NXB layout.", nameof(blocks));
        }
        Entries = [.. Enumerable.Range(0, blocks.Count).Select(index => new NxbBlock(blocks.GetBlock(index), format))];
    }

    internal NxbFile(byte[] bytes, NxbFormat format) : base(format)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length % format.BytesPerBlock != 0)
        {
            throw new InvalidDataException("The file must contain complete NXB blocks in the supplied layout.");
        }
        Entries = [.. bytes.Chunk(format.BytesPerBlock).Select(data => new NxbBlock(data, format))];
    }

    /// <summary>
    /// Gets the explicit NXB layout.
    /// </summary>
    public new NxbFormat Format => (NxbFormat)base.Format;
    /// <summary>
    /// Gets the ordered byte-backed blocks.
    /// </summary>
    public IReadOnlyList<NxbBlock> Entries { get; }
    /// <inheritdoc />
    public override TileBlocksData Blocks => new(Format.Width, Format.Height, [.. Entries.SelectMany(block => block.TileMap.Entries)]);
}