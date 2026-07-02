namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// Base class for blocks in a PZX file.
/// </summary>
public abstract class PzxBlock : Block<PzxBlockHeader>
{
    private protected PzxBlock(PzxBlockHeader header, byte[] data)
        : base(header, data)
    {
    }

    /// <inheritdoc />
    public override string ToString() => Header.ToString();
}

/// <summary>
/// Base class for blocks in a PZX file with a strongly-typed header.
/// </summary>
/// <typeparam name="THeader">The type of the block's header.</typeparam>
public abstract class PzxBlock<THeader> : PzxBlock
    where THeader : PzxBlockHeader
{
    private protected PzxBlock(THeader header, byte[] data) : base(header, data)
    {
    }

    /// <summary>
    /// Gets the strongly-typed header for this block.
    /// </summary>
    public new THeader Header => (THeader)base.Header;
}