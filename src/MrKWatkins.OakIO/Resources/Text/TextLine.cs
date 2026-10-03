namespace MrKWatkins.OakIO.Resources.Text;

/// <summary>
/// A byte-backed UTF-8 resource line, including its original line ending.
/// </summary>
public class TextLine : IOFileComponent
{
    internal TextLine(byte[] data) : base(data)
    {
    }

    /// <summary>
    /// Gets the line text without a UTF-8 byte-order mark or line ending.
    /// </summary>
    public string Text => PaletteText.Decode(AsReadOnlySpan()).TrimStart('\uFEFF').TrimEnd('\r', '\n');

    /// <summary>
    /// Gets the original line ending, or an empty string for an unterminated line.
    /// </summary>
    public string LineEnding => Data.Count == 0 ? "" : Data[^1] == (byte)'\n'
        ? Data.Count > 1 && Data[^2] == (byte)'\r' ? "\r\n" : "\n"
        : Data[^1] == (byte)'\r' ? "\r" : "";
}