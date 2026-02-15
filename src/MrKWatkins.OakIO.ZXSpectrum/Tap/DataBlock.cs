namespace MrKWatkins.OakIO.ZXSpectrum.Tap;

public sealed class DataBlock : TapBlock<DataHeader>
{
    internal DataBlock(DataHeader header, TapTrailer trailer, byte[] data)
        : base(header, trailer, data)
    {
    }

    [Pure]
    public static DataBlock Create([InstantHandle] IEnumerable<byte> data)
    {
        var (checksum, bytes) = CalculateChecksum(TapBlockType.Data, data);
        return new DataBlock(new DataHeader((ushort)(bytes.Length + 2)), new TapTrailer(checksum), bytes);
    }

    public override string ToString() => $"Data: {Header.BlockLength} bytes";
}