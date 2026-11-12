namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

/// <summary>
/// Base class for all ZX Spectrum tape files.
/// </summary>
/// <param name="format">The tape format of the file.</param>
public abstract class ZXSpectrumTapeFile(ZXSpectrumTapeFormat format) : ZXSpectrumFile(format)
{

    /// <summary>
    /// Gets the tape format of this file.
    /// </summary>
    public new ZXSpectrumTapeFormat Format => (ZXSpectrumTapeFormat)base.Format;
}