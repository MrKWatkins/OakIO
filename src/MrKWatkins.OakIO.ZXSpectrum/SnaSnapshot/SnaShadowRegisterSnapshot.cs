namespace MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;

internal sealed class SnaShadowRegisterSnapshot : ShadowRegisterSnapshot
{
    internal SnaShadowRegisterSnapshot(byte[] data)
        : base(data)
    {
    }

    public override ushort AF
    {
        get => GetWord(7);
        set => SetWord(7, value);
    }

    public override ushort BC
    {
        get => GetWord(5);
        set => SetWord(5, value);
    }

    public override ushort DE
    {
        get => GetWord(3);
        set => SetWord(3, value);
    }

    public override ushort HL
    {
        get => GetWord(1);
        set => SetWord(1, value);
    }
}