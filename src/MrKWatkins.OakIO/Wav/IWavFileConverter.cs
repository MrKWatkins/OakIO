namespace MrKWatkins.OakIO.Wav;

/// <summary>
/// Interface for converters that convert files to WAV format.
/// </summary>
public interface IWavFileConverter
{
    /// <summary>
    /// The default sample rate in Hz used for WAV conversion.
    /// </summary>
    public const uint DefaultSampleRateHz = 44100;

    /// <summary>
    /// Converts the specified source file to a WAV file.
    /// </summary>
    /// <param name="source">The source file to convert.</param>
    /// <param name="sampleRateHz">The sample rate in Hz for the output WAV file.</param>
    /// <returns>The converted WAV file.</returns>
    [Pure]
    WavFile Convert(IOFile source, uint sampleRateHz = DefaultSampleRateHz);
}