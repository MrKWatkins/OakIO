namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

internal sealed class SnaShadowRegisterSnapshot : ShadowRegisterSnapshot
{
    internal SnaShadowRegisterSnapshot(byte[] data)
        : base(data)
    {
    }

    public override ushort AF
    {
        get => GetUInt16(7);
        set => SetUInt16(7, value);
    }

    public override ushort BC
    {
        get => GetUInt16(5);
        set => SetUInt16(5, value);
    }

    public override ushort DE
    {
        get => GetUInt16(3);
        set => SetUInt16(3, value);
    }

    public override ushort HL
    {
        get => GetUInt16(1);
        set => SetUInt16(1, value);
    }
}