namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A single entry in a TZX archive info block.
/// </summary>
/// <param name="type">The type of archive info.</param>
/// <param name="text">The text value of the entry.</param>
public sealed class ArchiveInfoEntry(ArchiveInfoType type, string text)
{
    /// <summary>
    /// Gets the type of this archive info entry.
    /// </summary>
    public ArchiveInfoType Type { get; } = type;

    /// <summary>
    /// Gets the text value of this archive info entry.
    /// </summary>
    public string Text { get; } = text;

    /// <inheritdoc />
    public override string ToString() => $"{Type.ToDescription()}: {Text}";
}