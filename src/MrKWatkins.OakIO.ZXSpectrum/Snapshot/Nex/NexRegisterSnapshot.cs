namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class NexRegisterSnapshot : RegisterSnapshot
{
    internal NexRegisterSnapshot(NexHeader header)
        : base(header.Data.ToArray())
    {
    }

    public override ushort AF
    {
        get => 0;
        set { }
    }

    public override ushort BC
    {
        get => 0;
        set { }
    }

    public override ushort DE
    {
        get => 0;
        set { }
    }

    public override ushort HL
    {
        get => 0;
        set { }
    }

    public override ushort IX
    {
        get => 0;
        set { }
    }

    public override ushort IY
    {
        get => 0;
        set { }
    }

    public override ushort PC
    {
        get => GetUInt16(14);
        set => SetUInt16(14, value);
    }

    public override ushort SP
    {
        get => GetUInt16(12);
        set => SetUInt16(12, value);
    }

    public override ushort IR
    {
        get => 0;
        set { }
    }

    public override ShadowRegisterSnapshot Shadow => throw new NotSupportedException("NEX files do not contain shadow register data.");
}