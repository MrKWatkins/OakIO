namespace MrKWatkins.OakIO.ZXSpectrum;

/// <summary>
/// Provides access to all ZX Spectrum file formats and convenience methods for reading files.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ZXSpectrumFile : IOFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZXSpectrumFile" /> class.
    /// </summary>
    /// <param name="format">The ZX Spectrum format of the file.</param>
    protected ZXSpectrumFile(ZXSpectrumFileFormat format)
        : base(format)
    {
    }

    /// <summary>
    /// Gets the ZX Spectrum format of this file.
    /// </summary>
    public new ZXSpectrumFileFormat Format => (ZXSpectrumFileFormat)base.Format;
}