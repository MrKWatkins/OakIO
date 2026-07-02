namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// Base class for blocks in an RZX file.
/// </summary>
public abstract class RzxBlock : Block<RzxBlockHeader>
{
    private protected RzxBlock(RzxBlockHeader header, byte[] data)
        : base(header, data)
    {
    }

    /// <summary>
    /// Gets the type of the block.
    /// </summary>
    public RzxBlockType Type => Header.Type;

    /// <inheritdoc />
    [Pure]
    public override string ToString() => Header.ToString();

    /// <summary>
    /// Writes a fixed length, zero padded ASCII string into the target span.
    /// </summary>
    /// <param name="target">The destination span, whose length is the fixed field length. Any bytes beyond the string are left as zero.</param>
    /// <param name="value">The string to write.</param>
    protected static void WriteFixedAsciiString(Span<byte> target, string value)
    {
        if (value.Length > target.Length)
        {
            throw new ArgumentException($"Value \"{value}\" is longer than {target.Length} bytes.", nameof(value));
        }

        for (var f = 0; f < value.Length; f++)
        {
            var character = value[f];
            if (!char.IsAscii(character))
            {
                throw new ArgumentException($"Character at index {f} ('{character}') is not ASCII.", nameof(value));
            }

            target[f] = (byte)character;
        }
    }
}