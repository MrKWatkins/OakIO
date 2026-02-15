namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

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