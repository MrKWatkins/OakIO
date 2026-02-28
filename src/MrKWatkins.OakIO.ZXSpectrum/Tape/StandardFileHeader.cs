using System.Text;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

/// <summary>
/// Represents a standard ZX Spectrum ROM file header as found in the first 17 data bytes of a 19-byte tape block.
/// </summary>
public sealed class StandardFileHeader
{
    private const int BlockLength = 19;
    private const int FilenameLength = 10;

    internal StandardFileHeader(TapHeaderType type, string filename, ushort dataBlockLength, ushort parameter1, ushort parameter2)
    {
        Type = type;
        Filename = filename;
        DataBlockLength = dataBlockLength;
        Parameter1 = parameter1;
        Parameter2 = parameter2;
    }

    /// <summary>
    /// Gets the type of the header.
    /// </summary>
    [Pure]
    public TapHeaderType Type { get; }

    /// <summary>
    /// Gets the filename stored in the header.
    /// </summary>
    [Pure]
    public string Filename { get; }

    /// <summary>
    /// Gets the length of the associated data block in bytes.
    /// </summary>
    [Pure]
    public ushort DataBlockLength { get; }

    /// <summary>
    /// Gets the first parameter whose meaning depends on <see cref="Type" />.
    /// </summary>
    [Pure]
    public ushort Parameter1 { get; }

    /// <summary>
    /// Gets the second parameter whose meaning depends on <see cref="Type" />.
    /// </summary>
    [Pure]
    public ushort Parameter2 { get; }

    /// <summary>
    /// Attempts to create a <see cref="StandardFileHeader" /> from a 19-byte tape data block containing a flag byte,
    /// 17 header bytes, and a checksum byte.
    /// </summary>
    /// <param name="tapeData">The raw tape data including flag and checksum bytes.</param>
    /// <param name="header">The extracted header, or <c>null</c> if the data is not a valid standard file header.</param>
    /// <returns><c>true</c> if the data was a valid standard file header; <c>false</c> otherwise.</returns>
    internal static bool TryCreate(ReadOnlySpan<byte> tapeData, [NotNullWhen(true)] out StandardFileHeader? header)
    {
        header = null;

        if (tapeData.Length != BlockLength)
        {
            return false;
        }

        // Flag byte must be 0x00 (header).
        if (tapeData[0] != 0x00)
        {
            return false;
        }

        // Header type must be in range 0-3.
        if (tapeData[1] > 3)
        {
            return false;
        }

        // Verify checksum: XOR of all bytes should be 0.
        byte checksum = 0;
        for (var i = 0; i < tapeData.Length; i++)
        {
            checksum ^= tapeData[i];
        }

        if (checksum != 0)
        {
            return false;
        }

        var type = (TapHeaderType)tapeData[1];
        var filename = Encoding.ASCII.GetString(tapeData.Slice(2, FilenameLength)).TrimEnd();
        var dataBlockLength = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(tapeData[12..]);
        var parameter1 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(tapeData[14..]);
        var parameter2 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(tapeData[16..]);

        header = new StandardFileHeader(type, filename, dataBlockLength, parameter1, parameter2);
        return true;
    }
}