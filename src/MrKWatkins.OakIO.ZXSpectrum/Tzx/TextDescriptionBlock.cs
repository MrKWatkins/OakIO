namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class TextDescriptionBlock : TzxTextBlock<TextDescriptionHeader>
{
    public TextDescriptionBlock(Stream stream) : base(new TextDescriptionHeader(stream), stream)
    {
    }

    internal TextDescriptionBlock(byte[] headerData, byte[] data) : base(new TextDescriptionHeader(headerData), data)
    {
    }
}