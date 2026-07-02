namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing a free-form text description of the tape.
/// </summary>
public sealed class TextDescriptionBlock : TzxTextBlock<TextDescriptionHeader>
{
    internal TextDescriptionBlock(byte[] headerData, byte[] data) : base(new TextDescriptionHeader(headerData), data)
    {
    }
}