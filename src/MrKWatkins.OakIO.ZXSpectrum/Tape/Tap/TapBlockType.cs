namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// The type of a block in a TAP file.
/// </summary>
public enum TapBlockType
{
    /// <summary>
    /// A header block containing metadata about the following data block.
    /// </summary>
    Header = 0x00,

    /// <summary>
    /// A data block containing the actual file data.
    /// </summary>
    Data = 0xFF
}