using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

/// <summary>
/// A headerless tile or block map with externally supplied layout information.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/benbaker76/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c (write_map).
/// </remarks>
public sealed class NxmFile : TileMapFile
{
    /// <summary>
    /// Creates an NXM file, rejecting dimensions or attributes that the selected layout cannot represent.
    /// </summary>
    /// <param name="map">The row-major map.</param>
    /// <param name="format">The explicit output layout.</param>
    public NxmFile(TileMapData map, NxmFormat format) : base(format)
    {
        MapData = new NxmMapData(map, format);
    }

    internal NxmFile(byte[] bytes, NxmFormat format) : base(format)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length != format.Length)
        {
            throw new InvalidDataException("The file length must exactly match the supplied NXM dimensions and entry encoding.");
        }
        MapData = new NxmMapData(bytes, format);
    }

    /// <summary>
    /// Gets the explicit NXM layout.
    /// </summary>
    public new NxmFormat Format => (NxmFormat)base.Format;
    /// <summary>
    /// Gets the byte-backed map entries in native order.
    /// </summary>
    public NxmMapData MapData { get; }
    /// <inheritdoc />
    public override TileMapData TileMap => MapData.TileMap;
}