namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The header for a <see cref="DataBlock" />.
/// </summary>
public sealed class DataHeader : PzxBlockHeader
{
    internal DataHeader()
        : base(PzxBlockType.Data, 12)
    {
    }

    internal DataHeader(Stream stream)
        : base(PzxBlockType.Data, 12, stream)
    {
    }

    internal DataHeader(byte[] data)
        : base(PzxBlockType.Data, data)
    {
    }

    /// <summary>
    /// Gets the size of the data in bits.
    /// </summary>
    public uint SizeInBits => GetUInt32(StartIndex) & 0x7FFFFFFF;

    /// <summary>
    /// Gets the size of the data in whole bytes.
    /// </summary>
    public uint SizeInBytes => SizeInBits / 8;

    /// <summary>
    /// Gets the number of extra bits in the last byte.
    /// </summary>
    public uint ExtraBits => SizeInBits % 8;

    /// <summary>
    /// Gets a value indicating the initial pulse level.
    /// </summary>
    public bool InitialPulseLevel => GetBit(StartIndex + 3, 7);

    /// <summary>
    /// Gets the duration of the tail pulse in T-states.
    /// </summary>
    public ushort Tail => GetUInt16(StartIndex + 4);

    /// <summary>
    /// Gets the number of pulses in the zero bit pulse sequence.
    /// </summary>
    public byte NumberOfPulseInZeroBitSequence => GetByte(StartIndex + 6);

    /// <summary>
    /// Gets the number of pulses in the one bit pulse sequence.
    /// </summary>
    public byte NumberOfPulseInOneBitSequence => GetByte(StartIndex + 7);
}