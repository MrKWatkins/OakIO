namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Base class for headers of TZX blocks.
/// </summary>
public abstract class TzxBlockHeader : Header
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TzxBlockHeader"/> class with the specified type and size.
    /// </summary>
    /// <param name="type">The TZX block type.</param>
    /// <param name="size">The size of the header data in bytes.</param>
    protected TzxBlockHeader(TzxBlockType type, int size)
        : base(size)
    {
        Type = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TzxBlockHeader"/> class by reading from a stream.
    /// </summary>
    /// <param name="type">The TZX block type.</param>
    /// <param name="size">The size of the header data in bytes.</param>
    /// <param name="stream">The stream to read from.</param>
    protected TzxBlockHeader(TzxBlockType type, int size, Stream stream)
        : base(size, stream)
    {
        Type = type;
    }

    internal TzxBlockHeader(TzxBlockType type, byte[] data)
        : base(data)
    {
        Type = type;
    }

    /// <summary>
    /// Gets the type of this TZX block.
    /// </summary>
    public TzxBlockType Type { get; }

    /// <summary>
    /// Gets the length of the block data in bytes.
    /// </summary>
    public virtual int BlockLength => 0;

    /// <inheritdoc />
    public override string ToString() => Type.ToString();
}