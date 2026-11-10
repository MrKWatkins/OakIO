using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>Reads and writes Arkos AKY music with an explicit pointer origin.</summary>
/// <remarks>Format reference https://bitbucket.org/JulienNevo/arkostracker3/src/master/doc/export/AKY.md.</remarks>
public sealed class AkyFormat : MusicFormat<AkyFile>
{
    /// <summary>The format for song-relative pointers or exports assembled at address zero.</summary>
    public static readonly AkyFormat Instance = new();

    /// <summary>Creates a format with the origin used when the file was exported.</summary>
    /// <param name="origin">The address subtracted from stored pointers to obtain file offsets.</param>
    public AkyFormat(ushort origin = 0) : base("Arkos AKY Music", "aky")
    {
        Origin = origin;
    }

    /// <summary>Gets the external pointer origin, which is not encoded in AKY headers.</summary>
    public ushort Origin { get; }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new AkyFile(await reader.ReadToEndAsync().ConfigureAwait(false), this);

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(AkyFile file, IBinaryWriter writer)
    {
        if (file.Format.Origin != Origin)
        {
            throw new ArgumentException("AKY pointer origins must match when writing. Export with the desired origin instead.", nameof(file));
        }
        await file.Header.WriteAsync(writer).ConfigureAwait(false);
        await file.Body.WriteAsync(writer).ConfigureAwait(false);
    }
}