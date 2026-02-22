namespace MrKWatkins.OakIO.Tape;

public sealed class TapeFormat : IOFileFormat<TapeFile>
{
    public static readonly TapeFormat Instance = new();

    private TapeFormat()
        : base("Tape", "tape")
    {
    }

    public override IOFile Read(Stream stream) => throw new NotSupportedException("Tape files cannot be read.");

    protected override void Write(TapeFile file, Stream stream) => throw new NotSupportedException("Tape files cannot be written.");
}