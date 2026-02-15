using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public abstract class PzxBlock : Block<PzxBlockHeader>
{
    private protected PzxBlock(PzxBlockHeader header, Stream stream)
        : base(header, stream.ReadExactly(header.BlockLength))
    {
    }

    public override string ToString() => Header.ToString();
}

public abstract class PzxBlock<THeader> : PzxBlock
    where THeader : PzxBlockHeader
{
    private protected PzxBlock(THeader header, Stream stream) : base(header, stream)
    {
    }

    public new THeader Header => (THeader)base.Header;
}