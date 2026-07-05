namespace MrKWatkins.OakIO.Wav;

/// <summary>
/// Represents a WAV audio file.
/// </summary>
public sealed class WavFile : IOFile
{
    private readonly byte[] sampleData;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavFile" /> class.
    /// </summary>
    /// <param name="sampleRate">The sample rate of the audio in Hz.</param>
    /// <param name="sampleData">The raw sample data.</param>
    public WavFile(uint sampleRate, byte[] sampleData)
        : this(new WavHeader(sampleRate, sampleData.Length), sampleData)
    {
    }

    internal WavFile(WavHeader header, byte[] sampleData)
        : base(WavFormat.Instance)
    {
        Header = header;
        this.sampleData = sampleData;
    }

    /// <summary>
    /// Gets the WAV header.
    /// </summary>
    public WavHeader Header { get; }

    /// <summary>
    /// Gets the sample rate of the audio in Hz.
    /// </summary>
    public uint SampleRate => Header.SampleRate;

    /// <summary>
    /// Gets the raw sample data.
    /// </summary>
    public IReadOnlyList<byte> SampleData => sampleData;

    internal ReadOnlyMemory<byte> SampleDataMemory => sampleData;
}