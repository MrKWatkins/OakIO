namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The types of blocks in a PZX file.
/// </summary>
#pragma warning disable CA1028
// https://github.com/raxoft/pzxtools/blob/master/docs/pzx_format.txt.
[SuppressMessage("ReSharper", "ArrangeTrailingCommaInMultilineLists")]
public enum PzxBlockType : uint
{
    /// <summary>
    /// Unknown block type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///  PZXT - PZX header block.
    /// </summary>
    Header = 'P' << 24 | 'Z' << 16 | 'X' << 8 | 'T',

    /// <summary>
    ///  PULS - Pulse sequence.
    /// </summary>
    PulseSequence = 'P' << 24 | 'U' << 16 | 'L' << 8 | 'S',

    /// <summary>
    ///  DATA - Data block.
    /// </summary>
    Data = 'D' << 24 | 'A' << 16 | 'T' << 8 | 'A',

    /// <summary>
    /// PAUS - Pause.
    /// </summary>
    Pause = 'P' << 24 | 'A' << 16 | 'U' << 8 | 'S',

    /// <summary>
    /// BRWS - Browse point.
    /// </summary>
    BrowsePoint = 'B' << 24 | 'R' << 16 | 'W' << 8 | 'S',

    /// <summary>
    /// STOP - Stop tape command
    /// </summary>
    Stop = 'S' << 24 | 'T' << 16 | 'O' << 8 | 'P',
}
#pragma warning restore CA1028