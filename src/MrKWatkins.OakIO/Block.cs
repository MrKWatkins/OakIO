namespace MrKWatkins.OakIO;

public abstract class Block : FileComponent
{
    protected Block(Header header, Trailer trailer, int length)
        : base(new byte[length])
    {
        Header = header;
        Trailer = trailer;
    }

    protected Block(Header header, Trailer trailer, int length, Stream data)
        : base(length, data)
    {
        Header = header;
        Trailer = trailer;
    }

    protected Block(Header header, Trailer trailer, int length, [InstantHandle] IEnumerable<byte> data)
        : base(length, data)
    {
        Header = header;
        Trailer = trailer;
    }

    protected Block(Header header, Trailer trailer, byte[] data)
        : base(data)
    {
        Header = header;
        Trailer = trailer;
    }

    public Header Header { get; }

    public Trailer Trailer { get; }

    [MustUseReturnValue]
    public virtual bool TryLoadInto(Span<byte> memory) => false;

    public void LoadInto(Span<byte> memory)
    {
        if (!TryLoadInto(memory))
        {
            throw new IOException($"{GetType().Name} blocks cannot be loaded into memory.");
        }
    }
}

public abstract class Block<THeader, TTrailer> : Block
    where THeader : Header
    where TTrailer : Trailer
{
    protected Block(Header header, Trailer trailer, int length)
        : base(header, trailer, length)
    {
    }

    protected Block(Header header, Trailer trailer, int length, Stream data)
        : base(header, trailer, length, data)
    {
    }

    protected Block(Header header, Trailer trailer, int length, [InstantHandle] IEnumerable<byte> data)
        : base(header, trailer, length, data)
    {
    }

    protected Block(Header header, Trailer trailer, byte[] data)
        : base(header, trailer, data)
    {
    }

    public new THeader Header => (THeader)base.Header;

    public new TTrailer Trailer => (TTrailer)base.Trailer;
}

public abstract class Block<THeader> : Block<THeader, EmptyTrailer>
    where THeader : Header
{
    protected Block(Header header, int length)
        : base(header, EmptyTrailer.Instance, length)
    {
    }

    protected Block(Header header, int length, Stream data)
        : base(header, EmptyTrailer.Instance, length, data)
    {
    }

    protected Block(Header header, int length, [InstantHandle] IEnumerable<byte> data)
        : base(header, EmptyTrailer.Instance, length, data)
    {
    }

    protected Block(Header header, byte[] data)
        : base(header, EmptyTrailer.Instance, data)
    {
    }
}