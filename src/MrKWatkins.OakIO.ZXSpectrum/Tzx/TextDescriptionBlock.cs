namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class TextDescriptionBlock(Stream stream) : TzxTextBlock<TextDescriptionHeader>(new TextDescriptionHeader(stream), stream);