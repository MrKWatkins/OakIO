namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class StopTheTapeIf48KHeader : TzxBlockHeader
{
    private const int Size = 4;

    internal StopTheTapeIf48KHeader()
        : base(TzxBlockType.StopTheTapeIf48K, Size)
    {
    }

    internal StopTheTapeIf48KHeader(Stream stream)
        : base(TzxBlockType.StopTheTapeIf48K, Size, stream)
    {
    }

    internal StopTheTapeIf48KHeader(byte[] data)
        : base(TzxBlockType.StopTheTapeIf48K, data)
    {
    }
}