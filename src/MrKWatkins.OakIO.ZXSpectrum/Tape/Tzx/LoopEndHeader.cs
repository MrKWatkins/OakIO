namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX loop end block.
/// </summary>
public sealed class LoopEndHeader : TzxBlockHeader
{
    internal LoopEndHeader(byte[] data)
        : base(TzxBlockType.LoopEnd, data)
    {
    }
}