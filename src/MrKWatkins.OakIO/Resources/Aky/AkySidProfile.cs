namespace MrKWatkins.OakIO.Resources.Aky;

/// <summary>The native timing encoding for AKY SID volume modulation.</summary>
public enum AkySidProfile
{
    /// <summary>Amstrad CPC durations stored in one byte per shelf.</summary>
    Cpc = 0,

    /// <summary>Amstrad Plus or custom durations stored in one word per shelf.</summary>
    AmstradPlus = 1,

    /// <summary>Atari ST timer control and data bytes for each shelf.</summary>
    AtariSt = 2
}