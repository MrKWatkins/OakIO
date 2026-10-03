namespace MrKWatkins.OakIO.Resources.Bmp;

/// <summary>
/// The BMP colour table, stored as BGR-reserved RGBQUAD entries.
/// </summary>
public sealed class BmpPalette : IOFileComponent
{
    internal BmpPalette(byte[] data) : base(data)
    {
    }

    /// <summary>
    /// Gets the number of stored palette entries.
    /// </summary>

    public int Count => Length / 4;

    /// <summary>
    /// Gets an opaque-colour convenience view. Reserved bytes remain available in Data.
    /// </summary>

    public IReadOnlyList<Colour> Colours
    {
        get
        {
            var colours = new Colour[Count];
            for (var i = 0; i < Count; i++)
            {
                var offset = i * 4;
                colours[i] = new Colour(GetByte(offset + 2), GetByte(offset + 1), GetByte(offset));
            }
            return colours;
        }
    }
}