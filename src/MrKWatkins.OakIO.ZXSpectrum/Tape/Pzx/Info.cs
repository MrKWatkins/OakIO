namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// An information entry from a PZX header block.
/// </summary>
/// <param name="Type">The type of the information entry.</param>
/// <param name="Text">The text content of the information entry.</param>
public sealed record Info(string Type, string Text)
{
    /// <inheritdoc />
    public override string ToString() => $"{Type}: {Text}";
}