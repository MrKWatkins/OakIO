namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX group end block.
/// </summary>
public sealed class GroupEndHeader : TzxBlockHeader
{
    internal GroupEndHeader(byte[] data)
        : base(TzxBlockType.GroupEnd, data)
    {
    }
}