using System.Text;

namespace MrKWatkins.OakIO.Wav;

/// <summary>
/// File format for WAV audio files.
/// </summary>
public sealed class WavFormat : IOFileFormat<WavFile>
{
    private const int HeaderSize = 44;

    /// <summary>
    /// The singleton instance of the WAV file format.
    /// </summary>
    public static readonly WavFormat Instance = new();

    private WavFormat()
        : base("WAV Audio", "wav")
    {
    }

    /// <inheritdoc />
    public override WavFile Read(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.ASCII, true);

        var riff = reader.ReadBytes(4);
        if (riff is not [(byte)'R', (byte)'I', (byte)'F', (byte)'F'])
        {
            throw new InvalidDataException("Not a valid WAV file: missing RIFF header.");
        }

        // chunkSize
        _ = reader.ReadInt32();

        var wave = reader.ReadBytes(4);
        if (wave is not [(byte)'W', (byte)'A', (byte)'V', (byte)'E'])
        {
            throw new InvalidDataException("Not a valid WAV file: missing WAVE format.");
        }

        var fmt = reader.ReadBytes(4);
        if (fmt is not [(byte)'f', (byte)'m', (byte)'t', (byte)' '])
        {
            throw new InvalidDataException("Not a valid WAV file: missing fmt subchunk.");
        }

        var subChunk1Size = reader.ReadInt32();
        if (subChunk1Size != 16)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected fmt subchunk size of 16 but got {subChunk1Size}.");
        }

        var audioFormat = reader.ReadUInt16();
        if (audioFormat != 1)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected PCM audio format (1) but got {audioFormat}.");
        }

        var numChannels = reader.ReadUInt16();
        if (numChannels != 1)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected 1 channel but got {numChannels}.");
        }

        var sampleRate = reader.ReadUInt32();
        // byteRate
        _ = reader.ReadUInt32();
        // blockAlign
        _ = reader.ReadUInt16();
        var bitsPerSample = reader.ReadUInt16();
        if (bitsPerSample != 8)
        {
            throw new InvalidDataException($"Not a valid WAV file: expected 8 bits per sample but got {bitsPerSample}.");
        }

        var data = reader.ReadBytes(4);
        if (data is not [(byte)'d', (byte)'a', (byte)'t', (byte)'a'])
        {
            throw new InvalidDataException("Not a valid WAV file: missing data subchunk.");
        }

        var dataSize = reader.ReadInt32();
        var sampleData = reader.ReadBytes(dataSize);

        return new WavFile(sampleRate, sampleData);
    }

    /// <inheritdoc />
    protected override void Write(WavFile file, Stream stream)
    {
        if (stream.CanSeek)
        {
            WriteToSeekableStream(file, stream);
            return;
        }

        using var seekableStream = new MemoryStream();
        WriteToSeekableStream(file, seekableStream);

        seekableStream.Position = 0;
        seekableStream.CopyTo(stream);
    }

    private static void WriteToSeekableStream(WavFile file, Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.ASCII, true);

        var dataLength = file.SampleData.Length;

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(dataLength + HeaderSize - 8);
        writer.Write("WAVEfmt "u8);

        // fmt subchunk
        writer.Write(16);               // Subchunk1Size (PCM)
        writer.Write((ushort)1);        // AudioFormat (PCM)
        writer.Write((ushort)1);        // NumChannels (mono)
        writer.Write(file.SampleRate);  // SampleRate
        writer.Write(file.SampleRate);  // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
        writer.Write((ushort)1);        // BlockAlign (NumChannels * BitsPerSample/8)
        writer.Write((ushort)8);        // BitsPerSample

        // data subchunk
        writer.Write("data"u8);
        writer.Write(dataLength);
        writer.Write(file.SampleData);
    }
}