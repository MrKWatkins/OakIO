namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

public readonly record struct Pulse(ushort Count, uint Duration)
{
    public override string ToString() => $"{Count} x {Duration}";
}