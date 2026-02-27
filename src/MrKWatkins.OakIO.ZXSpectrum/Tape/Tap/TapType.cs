namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// The type of data described by a TAP header block.
/// </summary>
public enum TapHeaderType
{
    /// <summary>
    /// A BASIC program.
    /// </summary>
    Program = 0,

    /// <summary>
    /// A number array.
    /// </summary>
    NumberArray = 1,

    /// <summary>
    /// A character array.
    /// </summary>
    CharacterArray = 2,

    /// <summary>
    /// A block of machine code or screen data.
    /// </summary>
    Code = 3
}