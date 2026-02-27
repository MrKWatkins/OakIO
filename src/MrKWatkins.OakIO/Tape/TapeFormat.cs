namespace MrKWatkins.OakIO.Tape;

/// <summary>
/// File format for generic tape files.
/// </summary>
public sealed class TapeFormat : IOFileFormat<TapeFile>
{
    /// <summary>
    /// The singleton instance of <see cref="TapeFormat" />.
    /// </summary>
    public static readonly TapeFormat Instance = new();

    private TapeFormat()
        : base("Tape", "tape")
    {
    }

    /// <inheritdoc />
    public override bool CanRead => false;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override IOFile Read(Stream stream) => throw new NotSupportedException("Tape files cannot be read.");

    /// <inheritdoc />
    protected override void Write(TapeFile file, Stream stream) => throw new NotSupportedException("Tape files cannot be written.");
}