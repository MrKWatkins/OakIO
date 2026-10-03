using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrum.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources;

/// <summary>
/// Supported ZX Spectrum Next resource formats, including general resource formats.
/// </summary>
public static class ZXSpectrumNextResourceFileFormats
{
    /// <summary>
    /// Gets resource formats that do not require an externally supplied bit depth, separate from snapshots and tapes.
    /// </summary>
    public static readonly IReadOnlyList<ResourceFormat> AllFormats = [.. ZXSpectrumResourceFileFormats.AllFormats, NxpFormat.Instance];

    /// <summary>
    /// Gets resource formats including SPR and NXT at explicitly selected bit depths.
    /// </summary>
    /// <param name="spriteBitsPerPixel">The sprite bit depth: four or eight.</param>
    /// <param name="tileBitsPerPixel">The tile bit depth: one, four or eight.</param>
    /// <returns>The formats with one explicit interpretation for each headerless tile extension.</returns>
    [Pure]
    public static IReadOnlyList<ResourceFormat> WithTiles(int spriteBitsPerPixel, int tileBitsPerPixel) =>
        [.. AllFormats, SprFormat.ForBitsPerPixel(spriteBitsPerPixel), NxtFormat.ForBitsPerPixel(tileBitsPerPixel)];
}