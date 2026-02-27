namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// The secondary load screen mode for a NEX file.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
#pragma warning disable CA1028
public enum NexLoadScreenMode : byte
#pragma warning restore CA1028
{
    /// <summary>
    /// No secondary load screen.
    /// </summary>
    None = 0,

    /// <summary>
    /// Layer 2 320x256 screen.
    /// </summary>
    Layer2x320x256 = 1,

    /// <summary>
    /// Layer 2 640x256 screen.
    /// </summary>
    Layer2x640x256 = 2,

    /// <summary>
    /// Tilemode screen.
    /// </summary>
    Tilemode = 3
}