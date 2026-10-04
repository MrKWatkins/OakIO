using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Plus3Dos;

/// <summary>
/// A byte-backed 128-byte +3DOS header retaining BASIC metadata and reserved bytes.
/// </summary>
/// <remarks>
/// https://worldofspectrum.org/ZXSpectrum128%2B3Manual/chapter8pt27.html
/// </remarks>
public sealed class Plus3DosHeader : Header
{
    /// <summary>
    /// Reads and validates a complete header.
    /// </summary>
    public Plus3DosHeader(byte[] bytes) : base(128, bytes)
    {
        if (!AsReadOnlySpan()[..9].SequenceEqual("PLUS3DOS\x1A"u8) || Checksum != CalculateChecksum(bytes))
        {
            throw new InvalidDataException("Invalid +3DOS header signature or checksum.");
        }
    }

    /// <summary>
    /// Gets the signature.
    /// </summary>
    public string Signature => GetString(0, 8);
    /// <summary>
    /// Gets the issue number.
    /// </summary>
    public byte Issue => GetByte(9);
    /// <summary>
    /// Gets the version number.
    /// </summary>
    public byte Version => GetByte(10);
    /// <summary>
    /// Gets the complete file size including the header.
    /// </summary>
    public uint FileSize => GetUInt32(11);
    /// <summary>
    /// Gets the BASIC file type.
    /// </summary>
    public byte FileType => GetByte(15);
    /// <summary>
    /// Gets the sixteen-bit BASIC length field.
    /// </summary>
    public ushort BasicLength => GetUInt16(16);
    /// <summary>
    /// Gets BASIC parameter one.
    /// </summary>
    public ushort Parameter1 => GetUInt16(18);
    /// <summary>
    /// Gets BASIC parameter two.
    /// </summary>
    public ushort Parameter2 => GetUInt16(20);
    /// <summary>
    /// Gets the stored checksum.
    /// </summary>
    public byte Checksum => GetByte(127);

    /// <summary>
    /// Creates a CODE header for a screen payload.
    /// </summary>
    [Pure]
    public static Plus3DosHeader Create(int payloadLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(payloadLength);
        var bytes = new byte[128];
        "PLUS3DOS\x1A"u8.CopyTo(bytes);
        bytes[9] = 1;
        bytes.SetUInt32(11, checked((uint)payloadLength + 128));
        bytes[15] = 3;
        bytes.SetUInt16(16, unchecked((ushort)payloadLength));
        bytes.SetUInt16(18, 16384);
        bytes.SetUInt16(20, 32768);
        bytes[127] = CalculateChecksum(bytes);
        return new Plus3DosHeader(bytes);
    }

    [Pure]
    private static byte CalculateChecksum(byte[] bytes) => unchecked((byte)bytes.Take(127).Sum(value => value));
}