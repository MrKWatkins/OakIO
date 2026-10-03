using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Resources;

/// <summary>
/// Supported Spectrum resources, including general resources, separate from snapshots and tapes.
/// </summary>
public static class ZXSpectrumResourceFileFormats
{
    /// <summary>
    /// Gets all supported Spectrum resource formats.
    /// </summary>
    public static readonly IReadOnlyList<ResourceFormat> AllFormats = [.. ResourceFileFormats.AllFormats, ScrFormat.Instance];
}