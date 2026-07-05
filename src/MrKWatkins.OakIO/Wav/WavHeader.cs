namespace MrKWatkins.OakIO.Wav;

/// <summary>
/// The 44-byte canonical header of a mono, 8-bit PCM WAV file: the RIFF/WAVE chunk plus the fmt and data subchunk headers.
/// </summary>
public sealed class WavHeader : Header
{
    internal const int Size = 44;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavHeader" /> class for the given sample rate and sample data length.
    /// </summary>
    /// <param name="sampleRate">The sample rate of the audio in Hz.</param>
    /// <param name="dataLength">The length of the sample data in bytes.</param>
    public WavHeader(uint sampleRate, int dataLength)
        : base(Size)
    {
        SetString(0, 4, "RIFF");
        SetInt32(4, dataLength + Size - 8);     // Chunk size.
        SetString(8, 4, "WAVE");
        SetString(12, 4, "fmt ");
        SetInt32(16, 16);                       // fmt subchunk size (PCM).
        AudioFormat = 1;
        NumChannels = 1;
        SampleRate = sampleRate;
        ByteRate = sampleRate;                  // NumChannels * BitsPerSample / 8 == 1.
        BlockAlign = 1;
        BitsPerSample = 8;
        SetString(36, 4, "data");
        DataSize = dataLength;
    }

    internal WavHeader(byte[] data)
        : base(data)
    {
        var span = AsReadOnlySpan();

        if (!span[..4].SequenceEqual("RIFF"u8))
        {
            throw new InvalidDataException("Not a valid WAV file: missing RIFF header.");
        }

        if (!span[8..12].SequenceEqual("WAVE"u8))
        {
            throw new InvalidDataException("Not a valid WAV file: missing WAVE format.");
        }

        if (!span[12..16].SequenceEqual("fmt "u8))
        {
            throw new InvalidDataException("Not a valid WAV file: missing fmt subchunk.");
        }

        var subChunk1Size = GetInt32(16);
        if (subChunk1Size != 16)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected fmt subchunk size of 16 but got {subChunk1Size}.");
        }

        if (AudioFormat != 1)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected PCM audio format (1) but got {AudioFormat}.");
        }

        if (NumChannels != 1)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected 1 channel but got {NumChannels}.");
        }

        if (BitsPerSample != 8)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected 8 bits per sample but got {BitsPerSample}.");
        }

        if (!span[36..40].SequenceEqual("data"u8))
        {
            throw new InvalidDataException("Not a valid WAV file: missing data subchunk.");
        }

        if (DataSize < 0)
        {
            throw new InvalidDataException($"Not a valid WAV file: negative data subchunk size of {DataSize}.");
        }
    }

    /// <summary>
    /// Gets or sets the audio format code. Always 1 (PCM) for files this library can read and write.
    /// </summary>
    public ushort AudioFormat
    {
        get => GetUInt16(20);
        private init => SetUInt16(20, value);
    }

    /// <summary>
    /// Gets or sets the number of channels. Always 1 (mono) for files this library can read and write.
    /// </summary>
    public ushort NumChannels
    {
        get => GetUInt16(22);
        private init => SetUInt16(22, value);
    }

    /// <summary>
    /// Gets or sets the sample rate of the audio in Hz.
    /// </summary>
    public uint SampleRate
    {
        get => GetUInt32(24);
        private init => SetUInt32(24, value);
    }

    /// <summary>
    /// Gets or sets the byte rate: <see cref="SampleRate" /> * <see cref="NumChannels" /> * <see cref="BitsPerSample" /> / 8.
    /// </summary>
    public uint ByteRate
    {
        get => GetUInt32(28);
        private init => SetUInt32(28, value);
    }

    /// <summary>
    /// Gets or sets the block align: <see cref="NumChannels" /> * <see cref="BitsPerSample" /> / 8.
    /// </summary>
    public ushort BlockAlign
    {
        get => GetUInt16(32);
        private init => SetUInt16(32, value);
    }

    /// <summary>
    /// Gets or sets the number of bits per sample. Always 8 for files this library can read and write.
    /// </summary>
    public ushort BitsPerSample
    {
        get => GetUInt16(34);
        private init => SetUInt16(34, value);
    }

    /// <summary>
    /// Gets or sets the length of the sample data in bytes.
    /// </summary>
    public int DataSize
    {
        get => GetInt32(40);
        private init => SetInt32(40, value);
    }
}