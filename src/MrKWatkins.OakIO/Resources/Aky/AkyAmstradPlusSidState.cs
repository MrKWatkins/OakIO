namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>Native Amstrad Plus SID shelf durations for one channel.</summary>
public sealed class AkyAmstradPlusSidState : AkySidState
{
    internal AkyAmstradPlusSidState(byte lowShelfVolume, ushort highShelfDuration, ushort lowShelfDuration) : base(lowShelfVolume)
    {
        HighShelfDuration = highShelfDuration;
        LowShelfDuration = lowShelfDuration;
    }

    /// <inheritdoc />
    public override AkySidProfile Profile => AkySidProfile.AmstradPlus;

    /// <summary>Gets the encoded high-shelf duration in the export's timing units.</summary>
    public ushort HighShelfDuration { get; }

    /// <summary>Gets the encoded low-shelf duration in the export's timing units.</summary>
    public ushort LowShelfDuration { get; }
}