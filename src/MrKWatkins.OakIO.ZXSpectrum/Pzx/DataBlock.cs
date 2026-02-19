using System.Globalization;
using System.Runtime.InteropServices;

namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

// TODO: Variable length header, move pulses into header.
public sealed class DataBlock : PzxBlock<DataHeader>
{
    public DataBlock(Stream stream) : base(new DataHeader(stream), stream)
    {
    }

    internal DataBlock(byte[] headerData, byte[] bodyData) : base(new DataHeader(headerData), bodyData)
    {
    }

    private const int IndexOfZeroBitPulseSequence = 0;

    private int LengthOfZeroBitPulseSequence => Header.NumberOfPulseInZeroBitSequence * 2;

    private int IndexOfOneBitPulseSequence => IndexOfZeroBitPulseSequence + LengthOfZeroBitPulseSequence;

    private int LengthOfOneBitPulseSequence => Header.NumberOfPulseInOneBitSequence * 2;

    private int DataIndex => IndexOfOneBitPulseSequence + LengthOfOneBitPulseSequence;

    public ReadOnlySpan<ushort> ZeroBitPulseSequence => MemoryMarshal.Cast<byte, ushort>(AsSpan().Slice(IndexOfZeroBitPulseSequence, LengthOfZeroBitPulseSequence));

    public ReadOnlySpan<ushort> OneBitPulseSequence => MemoryMarshal.Cast<byte, ushort>(AsSpan().Slice(IndexOfOneBitPulseSequence, LengthOfOneBitPulseSequence));

    public int DataStreamSize => Header.BlockLength - DataIndex;

    public ReadOnlySpan<byte> DataStream => AsSpan()[DataIndex..];

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