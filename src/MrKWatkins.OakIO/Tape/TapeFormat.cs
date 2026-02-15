namespace MrKWatkins.OakIO.Tape;

public sealed class TapeFormat : FileFormat
{
    public static readonly TapeFormat Instance = new();

    private TapeFormat()
        : base("Tape", "tape")
    {
    }

    public override IOFile Read(Stream stream) => throw new NotSupportedException("Tape files cannot be read from a stream.");

    public override void Write(IOFile file, Stream stream) => throw new NotSupportedException("Tape files cannot be written to a stream.");
}
