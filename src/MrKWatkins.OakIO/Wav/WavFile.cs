namespace MrKWatkins.OakIO.Wav;

/// <summary>
/// Represents a WAV audio file.
/// </summary>
/// <param name="sampleRate">The sample rate of the audio in Hz.</param>
/// <param name="sampleData">The raw sample data.</param>
public sealed class WavFile(uint sampleRate, byte[] sampleData) : IOFile(WavFormat.Instance)
{
    /// <summary>
    /// Gets the sample rate of the audio in Hz.
    /// </summary>
    public uint SampleRate { get; } = sampleRate;

    /// <summary>
    /// Gets the raw sample data.
    /// </summary>
    public byte[] SampleData { get; } = sampleData;
}