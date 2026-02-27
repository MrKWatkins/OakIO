namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX group end block.
/// </summary>
public sealed class GroupEndHeader : TzxBlockHeader
{
    private const int Size = 0;

    internal GroupEndHeader()
        : base(TzxBlockType.GroupEnd, Size)
    {
    }

    internal GroupEndHeader(Stream stream)
        : base(TzxBlockType.GroupEnd, Size, stream)
    {
    }
}