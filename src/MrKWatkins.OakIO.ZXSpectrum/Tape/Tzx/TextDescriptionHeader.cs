namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX text description block.
/// </summary>
public sealed class TextDescriptionHeader : TzxBlockHeader
{
    internal TextDescriptionHeader(byte[] data)
        : base(TzxBlockType.TextDescription, data)
    {
    }

    /// <inheritdoc />
    public override int BlockLength => GetByte(0);
}