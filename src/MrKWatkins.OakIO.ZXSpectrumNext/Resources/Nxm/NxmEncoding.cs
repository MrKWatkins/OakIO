namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

/// <summary>
/// Explicit interpretations of headerless NXM entries.
/// </summary>
public enum NxmEncoding
{
    /// <summary>
    /// Plain eight-bit tile or block indices, without attributes.
    /// </summary>
    Index8,
    /// <summary>
    /// Plain little-endian sixteen-bit tile or block indices, without attributes.
    /// </summary>
    Index16,
    /// <summary>
    /// Tile byte followed by graphics attributes; bit zero of attributes is ULA priority.
    /// </summary>
    Tiles256,
    /// <summary>
    /// Tile byte followed by graphics attributes; bit zero of attributes is tile index bit eight.
    /// </summary>
    Tiles512,
    /// <summary>
    /// Tile byte followed by text attributes: seven-bit palette offset and ULA priority.
    /// </summary>
    Text256,
    /// <summary>
    /// Tile byte followed by text attributes: seven-bit palette offset and tile index bit eight.
    /// </summary>
    Text512
}