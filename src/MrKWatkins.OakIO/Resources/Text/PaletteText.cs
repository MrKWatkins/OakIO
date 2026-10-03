using System.Globalization;
using System.Text;

namespace MrKWatkins.OakIO.Resources.Text;

internal static class PaletteText
{
    private static readonly UTF8Encoding Utf8 = new(false, true);

    [Pure]
    internal static string Decode(ReadOnlySpan<byte> data)
    {
        try
        {
            return Utf8.GetString(data);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException("Palette text must be valid UTF-8.", exception);
        }
    }

    [Pure]
    internal static IReadOnlyList<TextLine> ReadLines(byte[] data)
    {
        // Validate the complete input, including comments, before interpreting individual lines.
        _ = Decode(data);
        var lines = new List<TextLine>();
        var start = 0;
        for (var f = 0; f < data.Length; f++)
        {
            if (data[f] is not ((byte)'\r' or (byte)'\n'))
            {
                continue;
            }
            if (data[f] == (byte)'\r' && f + 1 < data.Length && data[f + 1] == (byte)'\n')
            {
                f++;
            }
            lines.Add(new TextLine(data[start..(f + 1)]));
            start = f + 1;
        }
        if (start < data.Length)
        {
            lines.Add(new TextLine(data[start..]));
        }
        return lines;
    }

    [Pure]
    internal static (Colour Colour, string Name) ParseRgb(string text, bool allowName)
    {
        var tokens = text.Split((char[]?)null, 4, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 3 || (!allowName && tokens.Length != 3)
            || !byte.TryParse(tokens[0], NumberStyles.None, CultureInfo.InvariantCulture, out var red)
            || !byte.TryParse(tokens[1], NumberStyles.None, CultureInfo.InvariantCulture, out var green)
            || !byte.TryParse(tokens[2], NumberStyles.None, CultureInfo.InvariantCulture, out var blue))
        {
            throw new InvalidDataException("A palette entry must contain three RGB components between 0 and 255.");
        }
        return (new Colour(red, green, blue), tokens.Length == 4 ? tokens[3].Trim() : "");
    }

    [Pure]
    internal static Colour ParseArgb(string text)
    {
        var value = text.Trim();
        if (value.Length != 8 || !uint.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var argb))
        {
            throw new InvalidDataException("A Paint.NET entry must contain exactly eight hexadecimal digits (AARRGGBB).");
        }
        return new Colour((byte)(argb >> 16), (byte)(argb >> 8), (byte)argb, (byte)(argb >> 24));
    }

    internal static void ValidateOpaque(PaletteData palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        if (palette.Colours.Any(colour => colour.Alpha != 255))
        {
            throw new ArgumentException("This palette format does not support alpha.", nameof(palette));
        }
    }

    internal static void ValidateSingleLine(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        if (value.IndexOfAny(['\r', '\n']) >= 0)
        {
            throw new ArgumentException("Palette metadata must not contain line endings.", parameterName);
        }
    }

    [Pure]
    internal static byte[] Encode(string text) => Utf8.GetBytes(text);

    [Pure]
    internal static byte[] Join(IEnumerable<TextLine> lines) => lines.SelectMany(line => line.Data).ToArray();
}