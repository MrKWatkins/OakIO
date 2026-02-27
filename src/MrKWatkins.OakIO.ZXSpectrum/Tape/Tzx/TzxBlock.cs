using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Base class for all blocks in a TZX tape file.
/// </summary>
public abstract class TzxBlock : Block<TzxBlockHeader>
{
    private protected TzxBlock(TzxBlockHeader header, Stream stream)
        : base(header, stream.ReadExactly(header.BlockLength))
    {
    }

    private protected TzxBlock(TzxBlockHeader header, byte[] data)
        : base(header, data)
    {
    }

    /// <inheritdoc />
    public override string ToString() => Header.ToString();
}

/// <summary>
/// Base class for a TZX block with a strongly-typed header.
/// </summary>
/// <typeparam name="THeader">The type of the block header.</typeparam>
public abstract class TzxBlock<THeader> : TzxBlock
    where THeader : TzxBlockHeader
{
    private protected TzxBlock(THeader header, Stream stream) : base(header, stream)
    {
    }

    private protected TzxBlock(THeader header, byte[] data) : base(header, data)
    {
    }

    /// <summary>
    /// Gets the strongly-typed header for this block.
    /// </summary>
    public new THeader Header => (THeader)base.Header;
}