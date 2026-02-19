namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

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

    public uint SizeInBits => GetUInt32(StartIndex) & 0x7FFFFFFF;

    public uint SizeInBytes => SizeInBits / 8;

    public uint ExtraBits => SizeInBits % 8;

    public bool InitialPulseLevel => GetBit(StartIndex + 3, 7);

    public ushort Tail => GetWord(StartIndex + 4);

    public byte NumberOfPulseInZeroBitSequence => GetByte(StartIndex + 6);

    public byte NumberOfPulseInOneBitSequence => GetByte(StartIndex + 7);
}