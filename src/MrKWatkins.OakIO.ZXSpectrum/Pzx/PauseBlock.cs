namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class PauseBlock : PzxBlock<PauseHeader>
{
    public PauseBlock(Stream stream) : base(new PauseHeader(stream), stream)
    {
    }

    internal PauseBlock(byte[] headerData) : base(new PauseHeader(headerData), [])
    {
    }

    public override string ToString() => $"Pause: Initial Level = {(Header.InitialPulseLevel ? 1 : 0)}, Duration = {Header.Duration},";
}