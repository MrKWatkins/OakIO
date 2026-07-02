namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

// NEX files do not store shadow register data, so the values are stubbed to zero to match the main NEX registers.
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class NexShadowRegisterSnapshot() : ShadowRegisterSnapshot([])
{
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
}