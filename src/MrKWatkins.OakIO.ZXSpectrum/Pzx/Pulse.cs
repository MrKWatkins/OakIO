namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public readonly record struct Pulse(ushort Count, uint Duration)
{
    public override string ToString() => $"{Count} x {Duration}";
}