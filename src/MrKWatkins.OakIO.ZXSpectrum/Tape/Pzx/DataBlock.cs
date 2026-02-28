using System.Globalization;
using System.Runtime.InteropServices;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A PZX block containing tape data.
/// </summary>
public sealed class DataBlock : PzxBlock<DataHeader>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="DataBlock" /> class from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public DataBlock(Stream stream) : base(new DataHeader(stream), stream)
    {
    }

    internal DataBlock(byte[] headerData, byte[] bodyData) : base(new DataHeader(headerData), bodyData)
    {
    }

    private const int IndexOfZeroBitPulseSequence = 0;

    [Pure]
    private int LengthOfZeroBitPulseSequence => Header.NumberOfPulseInZeroBitSequence * 2;

    [Pure]
    private int IndexOfOneBitPulseSequence => IndexOfZeroBitPulseSequence + LengthOfZeroBitPulseSequence;

    [Pure]
    private int LengthOfOneBitPulseSequence => Header.NumberOfPulseInOneBitSequence * 2;

    [Pure]
    private int DataIndex => IndexOfOneBitPulseSequence + LengthOfOneBitPulseSequence;

    /// <summary>
    /// Gets the pulse sequence for a zero bit.
    /// </summary>
    [Pure]
    public ReadOnlySpan<ushort> ZeroBitPulseSequence => MemoryMarshal.Cast<byte, ushort>(AsSpan().Slice(IndexOfZeroBitPulseSequence, LengthOfZeroBitPulseSequence));

    /// <summary>
    /// Gets the pulse sequence for a one bit.
    /// </summary>
    [Pure]
    public ReadOnlySpan<ushort> OneBitPulseSequence => MemoryMarshal.Cast<byte, ushort>(AsSpan().Slice(IndexOfOneBitPulseSequence, LengthOfOneBitPulseSequence));

    /// <summary>
    /// Gets the size of the data stream in bytes.
    /// </summary>
    [Pure]
    public int DataStreamSize => Header.BlockLength - DataIndex;

    /// <summary>
    /// Gets the raw data stream.
    /// </summary>
    [Pure]
    public ReadOnlySpan<byte> DataStream => AsSpan()[DataIndex..];

    /// <summary>
    /// Attempts to interpret this block's data stream as a standard ZX Spectrum ROM file header.
    /// </summary>
    /// <param name="header">The extracted header, or <c>null</c> if the data is not a valid standard file header.</param>
    /// <returns><c>true</c> if the data was a valid standard file header; <c>false</c> otherwise.</returns>
    public bool TryGetStandardFileHeader([NotNullWhen(true)] out StandardFileHeader? header) =>
        StandardFileHeader.TryCreate(DataStream, out header);

    /// <inheritdoc />
    [Pure]
    public override string ToString() =>
        $"{Header.Type}: Initial Level = {(Header.InitialPulseLevel ? 1 : 0)}, " +
        $"Size = {Header.SizeInBytes}{(Header.ExtraBits != 0 ? $".{Header.ExtraBits}" : "")}, Tail = {Header.Tail}, " +
        $"Bit 0 = [{string.Join(", ", ToStrings(ZeroBitPulseSequence))}], " +
        $"Bit 1 = [{string.Join(", ", ToStrings(OneBitPulseSequence))}] ";

    [Pure]
    private static IReadOnlyList<string> ToStrings(ReadOnlySpan<ushort> words)
    {
        var strings = new List<string>();
        foreach (var word in words)
        {
            strings.Add(word.ToString(NumberFormatInfo.InvariantInfo));
        }

        return strings;
    }
}