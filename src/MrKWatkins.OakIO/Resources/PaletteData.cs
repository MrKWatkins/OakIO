namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// An immutable, ordered palette, independent of its on-disk representation.
/// </summary>
public sealed class PaletteData
{
    /// <summary>
    /// Initializes a palette by copying the specified colours.
    /// </summary>
    /// <param name="colours">The colours in palette-index order. At least one colour is required.</param>
    public PaletteData(IEnumerable<Colour> colours)
    {
        ArgumentNullException.ThrowIfNull(colours);
        var copy = colours.ToArray();
        if (copy.Length == 0)
        {
            throw new ArgumentException("A palette must contain at least one colour.", nameof(colours));
        }
        Colours = copy;
    }

    /// <summary>
    /// Gets the colours in palette-index order.
    /// </summary>
    public IReadOnlyList<Colour> Colours { get; }
}