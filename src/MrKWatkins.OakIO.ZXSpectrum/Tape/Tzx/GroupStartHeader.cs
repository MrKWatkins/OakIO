namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX group start block.
/// </summary>
public sealed class GroupStartHeader : TzxBlockHeader
{
    internal GroupStartHeader(byte[] data)
        : base(TzxBlockType.GroupStart, data)
    {
    }

    /// <inheritdoc />
    public override int BlockLength => GetByte(0);
}