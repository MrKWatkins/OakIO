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

    /// <summary>
    /// Initialises a new instance of the <see cref="PzxBlockHeader" /> class with the specified type and size.
    /// </summary>
    /// <param name="type">The block type.</param>
    /// <param name="sizeIncludingSizeField">The size of the header including the size field.</param>
    protected PzxBlockHeader(PzxBlockType type, int sizeIncludingSizeField)
        : base(sizeIncludingSizeField)
    {
        Type = type;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="PzxBlockHeader" /> class from a stream.
    /// </summary>
    /// <param name="type">The block type.</param>
    /// <param name="sizeIncludingSizeField">The size of the header including the size field.</param>
    /// <param name="stream">The stream to read from.</param>
    protected PzxBlockHeader(PzxBlockType type, int sizeIncludingSizeField, Stream stream)
        : base(sizeIncludingSizeField, stream)
    {
        Type = type;
    }

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
    public override string ToString() => Type.ToString();
}