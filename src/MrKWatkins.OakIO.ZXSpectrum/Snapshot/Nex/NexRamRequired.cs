namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// The amount of RAM required for a NEX file.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
#pragma warning disable CA1028
public enum NexRamRequired : byte
#pragma warning restore CA1028
{
    /// <summary>
    /// 768K of RAM required.
    /// </summary>
    Ram768K = 0,

    /// <summary>
    /// 1792K of RAM required.
    /// </summary>
    Ram1792K = 1
}