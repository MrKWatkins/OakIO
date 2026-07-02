namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// Base class for headers of blocks in a PZX file.
/// </summary>
public abstract class PzxBlockHeader : Header
{
    /// <summary>
    /// The start index of the block-specific data within the header, after the size field.
    /// </summary>
    protected const int StartIndex = 4;

    internal PzxBlockHeader(PzxBlockType type, byte[] data)
        : base(data)
    {
        Type = type;
    }

    /// <summary>
    /// Gets the type of this PZX block.
    /// </summary>
    public PzxBlockType Type { get; }

    /// <summary>
    /// Gets the size of the block excluding the tag and size field.
    /// </summary>
    public int SizeOfBlockExcludingTagAndSizeField => (int)GetUInt32(0);

    /// <summary>
    /// Gets the size of the header excluding the tag and size field.
    /// </summary>
    public int SizeOfHeaderExcludingTagAndSizeField => Data.Count - 4;

    /// <summary>
    /// Gets the length of the block's body data.
    /// </summary>
    public int BlockLength => SizeOfBlockExcludingTagAndSizeField - SizeOfHeaderExcludingTagAndSizeField;

    /// <inheritdoc />
    [Pure]
    public override string ToString() => Type.ToString();
}