namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>A native track entry containing a duration and register-block pointer.</summary>
public sealed class AkyTrackBlock
{
    private readonly AkyFile file;

    internal AkyTrackBlock(AkyFile file, int offset)
    {
        this.file = file;
        Offset = offset;
    }

    /// <summary>Gets the entry's file offset.</summary>
    public int Offset { get; }

    /// <summary>Gets its frame duration, interpreting a zero byte as 256.</summary>
    public int Duration => file.ReadByte(Offset) == 0 ? 256 : file.ReadByte(Offset);

    /// <summary>Gets the referenced compressed register block's file offset.</summary>
    public int RegisterBlockOffset => file.ReadPointer(Offset + 1);
}