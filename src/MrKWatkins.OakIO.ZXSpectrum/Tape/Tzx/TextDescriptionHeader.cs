namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX text description block.
/// </summary>
public sealed class TextDescriptionHeader : TzxBlockHeader
{
    private const int Size = 1;

    internal TextDescriptionHeader()
        : base(TzxBlockType.TextDescription, Size)
    {
    }

    internal TextDescriptionHeader(Stream stream)
        : base(TzxBlockType.TextDescription, Size, stream)
    {
    }

    internal TextDescriptionHeader(byte[] data)
        : base(TzxBlockType.TextDescription, data)
    {
    }

    /// <inheritdoc />
    public override int BlockLength => GetByte(0);
}