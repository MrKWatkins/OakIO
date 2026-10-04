namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

/// <summary>
/// The optional palette encoding in a native Next screen dump.
/// </summary>
public enum ScreenPaletteEncoding
{
    /// <summary>
    /// No palette bytes; interpret indices using the default RGB332 palette.
    /// </summary>
    None,
    /// <summary>
    /// One RGB332 byte per colour.
    /// </summary>
    Rgb332,
    /// <summary>
    /// RGB332 followed by the low blue bit and optional priority bit.
    /// </summary>
    Rgb333
}