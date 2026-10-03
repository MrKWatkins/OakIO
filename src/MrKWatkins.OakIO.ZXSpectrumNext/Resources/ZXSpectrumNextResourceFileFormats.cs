using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources;

/// <summary>
/// Supported ZX Spectrum Next resource formats, including general resource formats.
/// </summary>
public static class ZXSpectrumNextResourceFileFormats
{
    /// <summary>
    /// Gets all supported resource formats, separate from snapshots and tapes.
    /// </summary>
    public static readonly IReadOnlyList<ResourceFormat> AllFormats = [.. ResourceFileFormats.AllFormats, NxpFormat.Instance];
}