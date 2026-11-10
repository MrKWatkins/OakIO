namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>Native Amstrad CPC SID shelf durations for one channel.</summary>
public sealed class AkyCpcSidState : AkySidState
{
    internal AkyCpcSidState(byte lowShelfVolume, byte highShelfDuration, byte lowShelfDuration) : base(lowShelfVolume)
    {
        HighShelfDuration = highShelfDuration;
        LowShelfDuration = lowShelfDuration;
    }

    /// <inheritdoc />
    public override AkySidProfile Profile => AkySidProfile.Cpc;

    /// <summary>Gets the encoded high-shelf duration in the export's timing units.</summary>
    public byte HighShelfDuration { get; }

    /// <summary>Gets the encoded low-shelf duration in the export's timing units.</summary>
    public byte LowShelfDuration { get; }
}