namespace MrKWatkins.OakIO;

/// <summary>
/// Base class for a block in a file that is composed of a header, data and a trailer.
/// </summary>
public abstract class Block : IOFileComponent
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Block" /> class with zero-filled data of the specified length.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="trailer">The trailer for this block.</param>
    /// <param name="length">The length of the block data in bytes.</param>
    protected Block(Header header, Trailer trailer, int length)
        : base(new byte[length])
    {
        Header = header;
        Trailer = trailer;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block" /> class by reading data from a stream.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="trailer">The trailer for this block.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="data">The stream to read the block data from.</param>
    protected Block(Header header, Trailer trailer, int length, Stream data)
        : base(length, data)
    {
        Header = header;
        Trailer = trailer;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block" /> class from a sequence of bytes.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="trailer">The trailer for this block.</param>
    /// <param name="length">The expected length of the block data in bytes.</param>
    /// <param name="data">The bytes for this block.</param>
    protected Block(Header header, Trailer trailer, int length, [InstantHandle] IEnumerable<byte> data)
        : base(length, data)
    {
        Header = header;
        Trailer = trailer;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block" /> class from a byte array.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="trailer">The trailer for this block.</param>
    /// <param name="data">The raw byte data for this block.</param>
    protected Block(Header header, Trailer trailer, byte[] data)
        : base(data)
    {
        Header = header;
        Trailer = trailer;
    }

    /// <summary>
    /// Gets the header for this block.
    /// </summary>
    public Header Header { get; }

    /// <summary>
    /// Gets the trailer for this block.
    /// </summary>
    public Trailer Trailer { get; }

    /// <summary>
    /// Attempts to load the block data into the specified memory span.
    /// </summary>
    /// <param name="memory">The memory span to load the data into.</param>
    /// <returns><c>true</c> if the data was loaded successfully; <c>false</c> otherwise.</returns>
    [MustUseReturnValue]
    public virtual bool TryLoadInto(Span<byte> memory) => false;

    /// <summary>
    /// Loads the block data into the specified memory span.
    /// </summary>
    /// <param name="memory">The memory span to load the data into.</param>
    public void LoadInto(Span<byte> memory)
    {
        if (!TryLoadInto(memory))
        {
            throw new IOException($"{GetType().Name} blocks cannot be loaded into memory.");
        }
    }
}

/// <summary>
/// Base class for a block with strongly-typed header and trailer.
/// </summary>
/// <typeparam name="THeader">The type of header for this block.</typeparam>
/// <typeparam name="TTrailer">The type of trailer for this block.</typeparam>
public abstract class Block<THeader, TTrailer> : Block
    where THeader : Header
    where TTrailer : Trailer
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Block{THeader, TTrailer}" /> class with zero-filled data of the specified length.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="trailer">The trailer for this block.</param>
    /// <param name="length">The length of the block data in bytes.</param>
    protected Block(Header header, Trailer trailer, int length)
        : base(header, trailer, length)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block{THeader, TTrailer}" /> class by reading data from a stream.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="trailer">The trailer for this block.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="data">The stream to read the block data from.</param>
    protected Block(Header header, Trailer trailer, int length, Stream data)
        : base(header, trailer, length, data)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block{THeader, TTrailer}" /> class from a sequence of bytes.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="trailer">The trailer for this block.</param>
    /// <param name="length">The expected length of the block data in bytes.</param>
    /// <param name="data">The bytes for this block.</param>
    protected Block(Header header, Trailer trailer, int length, [InstantHandle] IEnumerable<byte> data)
        : base(header, trailer, length, data)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block{THeader, TTrailer}" /> class from a byte array.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="trailer">The trailer for this block.</param>
    /// <param name="data">The raw byte data for this block.</param>
    protected Block(Header header, Trailer trailer, byte[] data)
        : base(header, trailer, data)
    {
    }

    /// <summary>
    /// Gets the strongly-typed header for this block.
    /// </summary>
    public new THeader Header => (THeader)base.Header;

    /// <summary>
    /// Gets the strongly-typed trailer for this block.
    /// </summary>
    public new TTrailer Trailer => (TTrailer)base.Trailer;
}

/// <summary>
/// Base class for a block with a strongly-typed header and an empty trailer.
/// </summary>
/// <typeparam name="THeader">The type of header for this block.</typeparam>
public abstract class Block<THeader> : Block<THeader, EmptyTrailer>
    where THeader : Header
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Block{THeader}" /> class with zero-filled data of the specified length.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="length">The length of the block data in bytes.</param>
    protected Block(Header header, int length)
        : base(header, EmptyTrailer.Instance, length)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block{THeader}" /> class by reading data from a stream.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="data">The stream to read the block data from.</param>
    protected Block(Header header, int length, Stream data)
        : base(header, EmptyTrailer.Instance, length, data)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block{THeader}" /> class from a sequence of bytes.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="length">The expected length of the block data in bytes.</param>
    /// <param name="data">The bytes for this block.</param>
    protected Block(Header header, int length, [InstantHandle] IEnumerable<byte> data)
        : base(header, EmptyTrailer.Instance, length, data)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Block{THeader}" /> class from a byte array.
    /// </summary>
    /// <param name="header">The header for this block.</param>
    /// <param name="data">The raw byte data for this block.</param>
    protected Block(Header header, byte[] data)
        : base(header, EmptyTrailer.Instance, data)
    {
    }
}