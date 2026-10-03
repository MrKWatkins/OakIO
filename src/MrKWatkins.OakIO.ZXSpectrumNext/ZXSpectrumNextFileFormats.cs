using MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex;

namespace MrKWatkins.OakIO.ZXSpectrumNext;

/// <summary>
/// Supported file formats for the ZX Spectrum Next, including ZX Spectrum formats.
/// </summary>
public static class ZXSpectrumNextFileFormats
{
    /// <summary>
    /// All supported ZX Spectrum Next file formats.
    /// </summary>
    public static readonly IReadOnlyList<ZXSpectrumFileFormat> AllFormats = [.. ZXSpectrumFileFormat.AllFormats, NexFormat.Instance];
}