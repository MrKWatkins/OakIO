using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

/// <summary>
/// Reads and writes headerless NXM maps with explicitly supplied dimensions, encoding and order.
/// </summary>
/// <remarks>
/// Export reference: https://github.com/benbaker76/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c (write_map).
/// Hardware attributes: https://wiki.specnext.dev/Tilemap.
/// </remarks>
public sealed class NxmFormat : TileMapFormat<NxmFile>
{
    /// <summary>
    /// Initializes the interpretation of a raw NXM map; no options are stored in the file itself.
    /// </summary>
    /// <param name="width">The positive map width in entries.</param>
    /// <param name="height">The positive map height in entries.</param>
    /// <param name="encoding">The entry interpretation.</param>
    /// <param name="order">The on-disk entry traversal order.</param>
    public NxmFormat(int width, int height, NxmEncoding encoding, ResourceDataOrder order = ResourceDataOrder.RowMajor)
        : base("ZX Spectrum Next Map", "nxm")
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (!Enum.IsDefined(encoding))
        {
            throw new ArgumentOutOfRangeException(nameof(encoding));
        }
        if (!Enum.IsDefined(order))
        {
            throw new ArgumentOutOfRangeException(nameof(order));
        }
        Width = width;
        Height = height;
        Encoding = encoding;
        Order = order;
        Length = checked(width * height * BytesPerEntry);
    }

    /// <summary>
    /// Gets the map width in entries.
    /// </summary>
    public int Width { get; }
    /// <summary>
    /// Gets the map height in entries.
    /// </summary>
    public int Height { get; }
    /// <summary>
    /// Gets the selected entry interpretation.
    /// </summary>
    public NxmEncoding Encoding { get; }
    /// <summary>
    /// Gets the on-disk traversal order.
    /// </summary>
    public ResourceDataOrder Order { get; }
    /// <summary>
    /// Gets the number of bytes per entry.
    /// </summary>
    public int BytesPerEntry => Encoding == NxmEncoding.Index8 ? 1 : 2;
    /// <summary>
    /// Gets the required file length in bytes.
    /// </summary>
    public int Length { get; }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new NxmFile(await reader.ReadToEndAsync().ConfigureAwait(false), this);

    /// <inheritdoc />
    protected override ValueTask WriteAsync(NxmFile file, IBinaryWriter writer)
    {
        if (file.Format.Width != Width || file.Format.Height != Height || file.Format.Encoding != Encoding || file.Format.Order != Order)
        {
            throw new ArgumentException("The file layout does not match this NXM format.", nameof(file));
        }
        return file.MapData.WriteAsync(writer);
    }
}