namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class ArchiveInfoEntry(ArchiveInfoType type, string text)
{
    public ArchiveInfoType Type { get; } = type;

    public string Text { get; } = text;

    public override string ToString() => $"{Type.ToDescription()}: {Text}";
}