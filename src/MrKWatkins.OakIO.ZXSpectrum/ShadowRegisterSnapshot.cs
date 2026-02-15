namespace MrKWatkins.OakIO.ZXSpectrum;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract class ShadowRegisterSnapshot : Header
{
    private protected ShadowRegisterSnapshot(byte[] data)
        : base(data)
    {
    }

    public abstract ushort AF { get; set; }

    public abstract ushort BC { get; set; }

    public abstract ushort DE { get; set; }

    public abstract ushort HL { get; set; }
}