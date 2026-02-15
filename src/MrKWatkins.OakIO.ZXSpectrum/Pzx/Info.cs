namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed record Info(string Type, string Text)
{
    public override string ToString() => $"{Type}: {Text}";
}