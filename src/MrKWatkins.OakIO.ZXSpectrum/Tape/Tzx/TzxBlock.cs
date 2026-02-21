using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

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

    public override string ToString() => Header.ToString();
}

public abstract class TzxBlock<THeader> : TzxBlock
    where THeader : TzxBlockHeader
{
    private protected TzxBlock(THeader header, Stream stream) : base(header, stream)
    {
    }

    private protected TzxBlock(THeader header, byte[] data) : base(header, data)
    {
    }

    public new THeader Header => (THeader)base.Header;
}