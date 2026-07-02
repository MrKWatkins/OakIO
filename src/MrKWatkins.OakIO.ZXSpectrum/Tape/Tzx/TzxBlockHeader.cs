namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Base class for headers of TZX blocks.
/// </summary>
public abstract class TzxBlockHeader : Header
{
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
    [Pure]
    public override string ToString() => Type.ToString();
}