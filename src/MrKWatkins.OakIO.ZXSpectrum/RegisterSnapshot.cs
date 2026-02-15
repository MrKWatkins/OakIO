namespace MrKWatkins.OakIO.ZXSpectrum;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract class RegisterSnapshot : Header
{
    private protected RegisterSnapshot(byte[] data)
        : base(data)
    {
    }

    public abstract ushort AF { get; set; }

    public abstract ushort BC { get; set; }

    public abstract ushort DE { get; set; }

    public abstract ushort HL { get; set; }

    public abstract ushort IX { get; set; }

    public abstract ushort IY { get; set; }

    public abstract ushort PC { get; set; }

    public abstract ushort SP { get; set; }

    public abstract ushort IR { get; set; }

    public abstract ShadowRegisterSnapshot Shadow { get; }
}