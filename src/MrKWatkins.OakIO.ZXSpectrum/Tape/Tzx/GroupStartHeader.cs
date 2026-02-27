namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX group start block.
/// </summary>
public sealed class GroupStartHeader : TzxBlockHeader
{
    private const int Size = 1;

    internal GroupStartHeader()
        : base(TzxBlockType.GroupStart, Size)
    {
    }

    internal GroupStartHeader(Stream stream)
        : base(TzxBlockType.GroupStart, Size, stream)
    {
    }

    /// <inheritdoc />
    public override int BlockLength => GetByte(0);
}