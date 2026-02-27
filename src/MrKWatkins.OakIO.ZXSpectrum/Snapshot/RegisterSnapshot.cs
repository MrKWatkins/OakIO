namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot;

/// <summary>
/// A snapshot of the Z80 CPU registers.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract class RegisterSnapshot : Header
{
    private protected RegisterSnapshot(byte[] data)
        : base(data)
    {
    }

    /// <summary>
    /// Gets or sets the AF register pair.
    /// </summary>
    public abstract ushort AF { get; set; }

    /// <summary>
    /// Gets or sets the BC register pair.
    /// </summary>
    public abstract ushort BC { get; set; }

    /// <summary>
    /// Gets or sets the DE register pair.
    /// </summary>
    public abstract ushort DE { get; set; }

    /// <summary>
    /// Gets or sets the HL register pair.
    /// </summary>
    public abstract ushort HL { get; set; }

    /// <summary>
    /// Gets or sets the IX index register.
    /// </summary>
    public abstract ushort IX { get; set; }

    /// <summary>
    /// Gets or sets the IY index register.
    /// </summary>
    public abstract ushort IY { get; set; }

    /// <summary>
    /// Gets or sets the program counter.
    /// </summary>
    public abstract ushort PC { get; set; }

    /// <summary>
    /// Gets or sets the stack pointer.
    /// </summary>
    public abstract ushort SP { get; set; }

    /// <summary>
    /// Gets or sets the interrupt vector and refresh register pair.
    /// </summary>
    public abstract ushort IR { get; set; }

    /// <summary>
    /// Gets the shadow register snapshot.
    /// </summary>
    public abstract ShadowRegisterSnapshot Shadow { get; }
}