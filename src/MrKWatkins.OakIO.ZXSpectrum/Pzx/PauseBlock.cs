namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class PauseBlock(Stream stream) : PzxBlock<PauseHeader>(new PauseHeader(stream), stream)
{
    public override string ToString() => $"Pause: Initial Level = {(Header.InitialPulseLevel ? 1 : 0)}, Duration = {Header.Duration},";
}