using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Wav;

/// <summary>
/// File format for WAV audio files.
/// </summary>
public sealed class WavFormat : IOFileFormat<WavFile>
{
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
        var headerBytes = new byte[WavHeader.Size];
        stream.ReadExactly(headerBytes);
        var header = new WavHeader(headerBytes);

        var sampleData = new byte[header.DataSize];
        stream.ReadExactly(sampleData);

        return new WavFile(header.SampleRate, sampleData);
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(WavFile file, IBinaryWriter writer)
    {
        var header = new WavHeader(file.SampleRate, file.SampleData.Length);
        await header.WriteAsync(writer).ConfigureAwait(false);

        await writer.WriteAsync(file.SampleData).ConfigureAwait(false);
    }
}