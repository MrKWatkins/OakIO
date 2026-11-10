namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>One channel's active SID volume modulation, independently of ordinary AY registers.</summary>
/// <remarks>SID alternates the channel's ordinary volume with its low-shelf volume between frame ticks.</remarks>
public abstract class AkySidState
{
    private protected AkySidState(byte lowShelfVolume)
    {
        LowShelfVolume = lowShelfVolume;
    }

    /// <summary>Gets the native timing encoding.</summary>
    public abstract AkySidProfile Profile { get; }

    /// <summary>Gets the four-bit low-shelf volume.</summary>
    public byte LowShelfVolume { get; }
}