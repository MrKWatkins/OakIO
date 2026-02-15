namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public abstract class TzxBlockHeader : Header
{
    protected TzxBlockHeader(TzxBlockType type, int size)
        : base(size)
    {
        Type = type;
    }

    protected TzxBlockHeader(TzxBlockType type, int size, Stream stream)
        : base(size, stream)
    {
        Type = type;
    }

    public TzxBlockType Type { get; }

    public virtual int BlockLength => 0;

    public override string ToString() => Type.ToString();
}