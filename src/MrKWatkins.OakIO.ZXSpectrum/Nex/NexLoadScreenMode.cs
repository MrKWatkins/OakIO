namespace MrKWatkins.OakIO.ZXSpectrum.Nex;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
#pragma warning disable CA1028
public enum NexLoadScreenMode : byte
#pragma warning restore CA1028
{
    None = 0,
    Layer2x320x256 = 1,
    Layer2x640x256 = 2,
    Tilemode = 3
}