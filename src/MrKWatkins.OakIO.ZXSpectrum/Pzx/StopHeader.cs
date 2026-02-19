namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class StopHeader : PzxBlockHeader
{
    internal StopHeader()
        : base(PzxBlockType.Stop, 6)
    {
    }

    internal StopHeader(Stream stream)
        : base(PzxBlockType.Stop, 6, stream)
    {
    }

    internal StopHeader(byte[] data)
        : base(PzxBlockType.Stop, data)
    {
    }

    // ReSharper disable once InconsistentNaming
    public bool Only48k => GetWord(StartIndex) == 1;
}