namespace MrKWatkins.OakIO.ZXSpectrum.Recording;

/// <summary>
/// Base class for ZX Spectrum input recording files.
/// </summary>
public abstract class ZXSpectrumRecordingFile : ZXSpectrumFile
{
    /// <summary>
    /// Initialises a new instance of the <see cref="ZXSpectrumRecordingFile" /> class.
    /// </summary>
    /// <param name="format">The recording format of the file.</param>
    private protected ZXSpectrumRecordingFile(ZXSpectrumRecordingFormat format)
        : base(format)
    {
    }

    /// <summary>
    /// Gets the recording format of this file.
    /// </summary>
    public new ZXSpectrumRecordingFormat Format => (ZXSpectrumRecordingFormat)base.Format;
}