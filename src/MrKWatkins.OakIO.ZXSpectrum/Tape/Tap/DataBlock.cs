namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// A data block in a TAP file.
/// </summary>
public sealed class DataBlock : TapBlock<DataHeader>
{
    internal DataBlock(DataHeader header, TapTrailer trailer, byte[] data)
        : base(header, trailer, data)
    {
    }

    /// <summary>
    /// Creates a new <see cref="DataBlock" /> from the specified data.
    /// </summary>
    /// <param name="data">The data for the block.</param>
    /// <returns>A new <see cref="DataBlock" /> containing the data with a calculated checksum.</returns>
    [Pure]
    public static DataBlock Create([InstantHandle] IEnumerable<byte> data)
    {
        var (checksum, bytes) = CalculateChecksum(TapBlockType.Data, data);
        return new DataBlock(new DataHeader((ushort)(bytes.Length + 2)), new TapTrailer(checksum), bytes);
    }

    /// <inheritdoc />
    public override string ToString() => $"Data: {Header.BlockLength} bytes";
}