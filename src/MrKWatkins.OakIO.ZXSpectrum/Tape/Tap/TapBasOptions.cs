namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Controls the BASIC workspace and optional entry call for a TapBAS loader.
/// </summary>
public sealed record TapBasOptions
{
    /// <summary>
    /// The highest address BASIC may use, passed to CLEAR. Defaults to $7FFF.
    /// This does not establish a machine-code program's stack pointer.
    /// </summary>
    public ushort ClearAddress { get; init; } = 0x7FFF;

    /// <summary>
    /// The address called with RANDOMIZE USR after loading, or null to leave the program in BASIC.
    /// </summary>
    public ushort? EntryPoint { get; init; }

    /// <summary>
    /// The RAM bank to select after loading. Defaults to bank 0 when any block uses paging;
    /// with no banked blocks and no explicit entry bank, no paging commands are emitted.
    /// </summary>
    public byte? EntryBank { get; init; }
}