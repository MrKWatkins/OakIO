namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

/// <summary>
/// Base class for all ZX Spectrum tape files.
/// </summary>
public abstract class ZXSpectrumTapeFile : ZXSpectrumFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZXSpectrumTapeFile" /> class.
    /// </summary>
    /// <param name="format">The tape format of the file.</param>
    protected ZXSpectrumTapeFile(ZXSpectrumTapeFormat format)
        : base(format)
    {
    }

    /// <summary>
    /// Gets the tape format of this file.
    /// </summary>
    public new ZXSpectrumTapeFormat Format => (ZXSpectrumTapeFormat)base.Format;
}