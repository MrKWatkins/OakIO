namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Base class for blocks in a TAP file.
/// </summary>
public abstract class TapBlock : Block<TapHeader, TapTrailer>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="TapBlock" /> class.
    /// </summary>
    /// <param name="header">The header for the block.</param>
    /// <param name="trailer">The trailer for the block.</param>
    /// <param name="data">The data contained in the block.</param>
    private protected TapBlock(TapHeader header, TapTrailer trailer, byte[] data)
        : base(header, trailer, data)
    {
    }

    /// <summary>
    /// Calculates the XOR checksum for the specified block type and data.
    /// </summary>
    /// <param name="type">The block type to include in the checksum.</param>
    /// <param name="data">The data to checksum.</param>
    /// <returns>A tuple containing the calculated checksum and the data as a byte array.</returns>
    [Pure]
    protected static (byte Checksum, byte[] Data) CalculateChecksum(TapBlockType type, [InstantHandle] IEnumerable<byte> data)
    {
        var result = new List<byte>();
        var checksum = (byte)type;
        foreach (var @byte in data)
        {
            checksum ^= @byte;
            result.Add(@byte);
        }
        return (checksum, result.ToArray());
    }

    /// <summary>
    /// Gets the XOR checksum of the block data.
    /// </summary>
    [Pure]
    public byte Checksum => Data.Aggregate((byte)Header.Type, (current, b) => (byte)(current ^ b));
}

/// <summary>
/// Base class for blocks in a TAP file with a strongly-typed header.
/// </summary>
/// <typeparam name="THeader">The type of header for the block.</typeparam>
public abstract class TapBlock<THeader> : TapBlock
    where THeader : TapHeader
{
    /// <summary>
    /// Initialises a new instance of the <see cref="TapBlock{THeader}" /> class.
    /// </summary>
    /// <param name="header">The header for the block.</param>
    /// <param name="trailer">The trailer for the block.</param>
    /// <param name="data">The data contained in the block.</param>
    private protected TapBlock(THeader header, TapTrailer trailer, byte[] data)
        : base(header, trailer, data)
    {
    }

    /// <summary>
    /// Gets the strongly-typed header for the block.
    /// </summary>
    public new THeader Header => (THeader)base.Header;
}