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
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var header = new WavHeader(await reader.ReadAsync(WavHeader.Size).ConfigureAwait(false));

        var sampleData = await reader.ReadAsync(header.DataSize).ConfigureAwait(false);

        return new WavFile(header, sampleData);
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(WavFile file, IBinaryWriter writer)
    {
        await file.Header.WriteAsync(writer).ConfigureAwait(false);

        await writer.WriteAsync(file.SampleDataMemory).ConfigureAwait(false);
    }
}