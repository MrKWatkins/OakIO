namespace MrKWatkins.OakIO.ZXSpectrum;

/// <summary>
/// Provides access to all ZX Spectrum file formats and convenience methods for reading files.
/// </summary>
/// <param name="format">The ZX Spectrum format of the file.</param>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ZXSpectrumFile(ZXSpectrumFileFormat format) : IOFile(format)
{

    /// <summary>
    /// Gets the ZX Spectrum format of this file.
    /// </summary>
    public new ZXSpectrumFileFormat Format => (ZXSpectrumFileFormat)base.Format;
}