using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb;

/// <summary>
/// Reads and writes headerless collections of row-major tile-index blocks with an explicit layout.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/benbaker76/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c (write_blocks and get_block).
/// </remarks>
public sealed class NxbFormat : TileBlocksFormat<NxbFile>
{
    /// <summary>
    /// Initializes the externally supplied block dimensions and plain index width.
    /// </summary>
    /// <param name="width">The positive width of each block in tile entries.</param>
    /// <param name="height">The positive height of each block in tile entries.</param>
    /// <param name="bitsPerIndex">The tile index width: eight or sixteen.</param>
    public NxbFormat(int width, int height, int bitsPerIndex) : base("ZX Spectrum Next Blocks", "nxb")
    {
        if (bitsPerIndex is not (8 or 16))
        {
            throw new ArgumentOutOfRangeException(nameof(bitsPerIndex), bitsPerIndex, "NXB indices must be eight or sixteen bits.");
        }
        MapFormat = new NxmFormat(width, height, bitsPerIndex == 8 ? NxmEncoding.Index8 : NxmEncoding.Index16);
        BitsPerIndex = bitsPerIndex;
    }

    internal NxmFormat MapFormat { get; }
    /// <summary>
    /// Gets the width of each block in tile entries.
    /// </summary>
    public int Width => MapFormat.Width;
    /// <summary>
    /// Gets the height of each block in tile entries.
    /// </summary>
    public int Height => MapFormat.Height;
    /// <summary>
    /// Gets the width of each plain tile index in bits.
    /// </summary>
    public int BitsPerIndex { get; }
    /// <summary>
    /// Gets the number of bytes in a complete block.
    /// </summary>
    public int BytesPerBlock => MapFormat.Length;

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new NxbFile(await reader.ReadToEndAsync().ConfigureAwait(false), this);

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(NxbFile file, IBinaryWriter writer)
    {
        if (file.Format.Width != Width || file.Format.Height != Height || file.Format.BitsPerIndex != BitsPerIndex)
        {
            throw new ArgumentException("The file layout does not match this NXB format.", nameof(file));
        }
        foreach (var block in file.Entries)
        {
            await block.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}