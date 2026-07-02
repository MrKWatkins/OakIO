namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// The header for a block in an RZX file, consisting of a block ID byte and the total block length.
/// </summary>
public sealed class RzxBlockHeader : Header
{
    /// <summary>
    /// The length of an RZX block header in bytes.
    /// </summary>
    internal const int Size = 5;

    internal RzxBlockHeader(RzxBlockType type, uint dataLength)
        : base(Size)
    {
        Type = type;
        BlockLength = checked(dataLength + Size);
    }

    internal RzxBlockHeader(byte[] data)
        : base(data)
    {
        if (BlockLength < Size)
        {
            throw new InvalidDataException("RZX block length must include the five byte block header.");
        }
    }

    /// <summary>
    /// Gets the type of the block.
    /// </summary>
    public RzxBlockType Type
    {
        get => GetByte<RzxBlockType>(0);
        private init => SetByte(0, value);
    }

    /// <summary>
    /// Gets the total length of the block, including this header.
    /// </summary>
    public uint BlockLength
    {
        get => GetUInt32(1);
        private init => SetUInt32(1, value);
    }

    /// <summary>
    /// Gets the length of the block's body data, excluding this header.
    /// </summary>
    public uint DataLength => BlockLength - Size;

    /// <inheritdoc />
    [Pure]
    public override string ToString() => Type.ToString();
}