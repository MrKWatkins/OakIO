namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX loop end block.
/// </summary>
public sealed class LoopEndHeader : TzxBlockHeader
{
    private const int Size = 0;

    internal LoopEndHeader()
        : base(TzxBlockType.LoopEnd, Size)
    {
    }

    internal LoopEndHeader(Stream stream)
        : base(TzxBlockType.LoopEnd, Size, stream)
    {
    }
}