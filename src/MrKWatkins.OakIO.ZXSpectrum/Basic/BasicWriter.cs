using System.Globalization;
using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Basic;

internal sealed class BasicWriter
{
    private readonly Stream stream;

    internal BasicWriter(Stream stream)
    {
        this.stream = stream;
    }

    internal void WriteLine(int lineNumber, params object[] symbols)
    {
        var line = new List<byte>();
        Append(line, symbols);

        stream.WriteWord((ushort)lineNumber, Endian.Big);
        stream.WriteWord((ushort)(line.Count + 1));  // +1 for newline.

        foreach (var symbol in line)
        {
            stream.WriteByte(symbol);
        }
        stream.WriteByte(0x0D);
    }

    private static void Append(List<byte> line, [InstantHandle] IEnumerable<object> symbols)
    {
        foreach (var symbol in symbols)
        {
            switch (symbol)
            {
                case byte b:
                    Append(line, b);
                    break;

                case char c:
                    Append(line, c);
                    break;

                case int i:
                    Append(line, (ushort)i);
                    break;

                case string s:
                    Append(line, s);
                    break;

                case ushort w:
                    Append(line, w);
                    break;

                default:
                    throw new NotSupportedException($"The symbol type {symbol.GetType()} is not supported.");
            }
        }
    }

    private static void Append(List<byte> line, byte value) => line.Add(value);

    private static void Append(List<byte> line, char value) => line.Add((byte)value);

    private static void Append(List<byte> line, string value)
    {
        Append(line, '"');
        foreach (var character in value)
        {
            Append(line, character);
        }
        Append(line, '"');
    }

    private static void Append(List<byte> line, ushort value)
    {
        // BASIC has the string as ASCII, following by the number itself.
        foreach (var character in value.ToString(NumberFormatInfo.InvariantInfo))
        {
            Append(line, character);
        }

        Append(line, 0x0E);  // Number prefix.

        // 5 byte floating point number.
        Append(line, 0x00);
        Append(line, 0x00);
        Append(line, value.LeastSignificantByte());
        Append(line, value.MostSignificantByte());
        Append(line, 0x00);
    }
}