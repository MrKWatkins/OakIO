namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot;

/// <summary>
/// A snapshot of the Z80 CPU shadow registers.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract class ShadowRegisterSnapshot : Header
{
    private protected ShadowRegisterSnapshot(byte[] data)
        : base(data)
    {
    }

    /// <summary>
    /// Gets or sets the shadow AF' register pair.
    /// </summary>
    public abstract ushort AF { get; set; }

    /// <summary>
    /// Gets or sets the shadow BC' register pair.
    /// </summary>
    public abstract ushort BC { get; set; }

    /// <summary>
    /// Gets or sets the shadow DE' register pair.
    /// </summary>
    public abstract ushort DE { get; set; }

    /// <summary>
    /// Gets or sets the shadow HL' register pair.
    /// </summary>
    public abstract ushort HL { get; set; }
}