using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb;

/// <summary>
/// A byte-backed row-major rectangle of plain tile indices, without tile attributes.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/benbaker76/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c (write_blocks and get_block).
/// </remarks>
public sealed class NxbBlock : IOFileComponent
{
    internal NxbBlock(byte[] bytes, NxbFormat format) : base(format.BytesPerBlock, bytes) => Format = format;

    internal NxbBlock(TileMapData map, NxbFormat format) : this([.. new NxmMapData(map, format.MapFormat).Data], format)
    {
    }

    /// <summary>
    /// Gets the explicit block layout.
    /// </summary>
    public NxbFormat Format { get; }
    /// <summary>
    /// Gets a derived map view of this block's plain indices.
    /// </summary>
    public TileMapData TileMap => new NxmMapData([.. Data], Format.MapFormat).TileMap;
}