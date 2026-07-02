namespace MrKWatkins.OakIO.Wav;

/// <summary>
/// The 44-byte canonical header of a mono, 8-bit PCM WAV file: the RIFF/WAVE chunk plus the fmt and data subchunk headers.
/// </summary>
internal sealed class WavHeader : Header
{
    internal const int Size = 44;

    internal WavHeader(uint sampleRate, int dataLength)
        : base(Size)
    {
        SetString(0, 4, "RIFF");
        SetInt32(4, dataLength + Size - 8);     // Chunk size.
        SetString(8, 4, "WAVE");
        SetString(12, 4, "fmt ");
        SetInt32(16, 16);                       // fmt subchunk size (PCM).
        SetUInt16(20, 1);                       // Audio format (PCM).
        SetUInt16(22, 1);                       // Number of channels (mono).
        SetUInt32(24, sampleRate);              // Sample rate.
        SetUInt32(28, sampleRate);              // Byte rate = sample rate * channels * bits per sample / 8.
        SetUInt16(32, 1);                       // Block align = channels * bits per sample / 8.
        SetUInt16(34, 8);                       // Bits per sample.
        SetString(36, 4, "data");
        SetInt32(40, dataLength);               // Data subchunk size.
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

        var audioFormat = GetUInt16(20);
        if (audioFormat != 1)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected PCM audio format (1) but got {audioFormat}.");
        }

        var numChannels = GetUInt16(22);
        if (numChannels != 1)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected 1 channel but got {numChannels}.");
        }

        var bitsPerSample = GetUInt16(34);
        if (bitsPerSample != 8)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected 8 bits per sample but got {bitsPerSample}.");
        }

        if (!span[36..40].SequenceEqual("data"u8))
        {
            throw new InvalidDataException("Not a valid WAV file: missing data subchunk.");
        }

        var dataSize = GetInt32(40);
        if (dataSize < 0)
        {
            throw new InvalidDataException($"Not a valid WAV file: negative data subchunk size of {dataSize}.");
        }
    }

    internal uint SampleRate => GetUInt32(24);

    internal int DataSize => GetInt32(40);
}