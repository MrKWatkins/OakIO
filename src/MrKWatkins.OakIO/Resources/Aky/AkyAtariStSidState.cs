namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>Native Atari ST SID timer bytes for one channel.</summary>
/// <remarks>The exporter writes four individual bytes, independently of the file's word byte order.</remarks>
public sealed class AkyAtariStSidState : AkySidState
{
    internal AkyAtariStSidState(byte lowShelfVolume, byte highShelfTimerControl, byte highShelfTimerData, byte lowShelfTimerControl, byte lowShelfTimerData) : base(lowShelfVolume)
    {
        HighShelfTimerControl = highShelfTimerControl;
        HighShelfTimerData = highShelfTimerData;
        LowShelfTimerControl = lowShelfTimerControl;
        LowShelfTimerData = lowShelfTimerData;
    }

    /// <inheritdoc />
    public override AkySidProfile Profile => AkySidProfile.AtariSt;

    /// <summary>Gets the high-shelf MFP timer control byte.</summary>
    public byte HighShelfTimerControl { get; }

    /// <summary>Gets the high-shelf MFP timer data byte.</summary>
    public byte HighShelfTimerData { get; }

    /// <summary>Gets the low-shelf MFP timer control byte.</summary>
    public byte LowShelfTimerControl { get; }

    /// <summary>Gets the low-shelf MFP timer data byte.</summary>
    public byte LowShelfTimerData { get; }
}