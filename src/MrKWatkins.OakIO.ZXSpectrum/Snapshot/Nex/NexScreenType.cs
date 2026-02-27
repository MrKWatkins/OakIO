namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// The type of loading screen in a NEX file.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum NexScreenType
{
    /// <summary>
    /// Layer 2 screen.
    /// </summary>
    Layer2,

    /// <summary>
    /// ULA screen.
    /// </summary>
    Ula,

    /// <summary>
    /// Lo-res screen.
    /// </summary>
    LoRes,

    /// <summary>
    /// Hi-res screen.
    /// </summary>
    HiRes,

    /// <summary>
    /// Hi-colour screen.
    /// </summary>
    HiColour,

    /// <summary>
    /// Layer 2 320x256 screen.
    /// </summary>
    Layer2x320x256,

    /// <summary>
    /// Layer 2 640x256 screen.
    /// </summary>
    Layer2x640x256
}