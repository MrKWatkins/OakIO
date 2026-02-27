namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing a free-form text description of the tape.
/// </summary>
public sealed class TextDescriptionBlock : TzxTextBlock<TextDescriptionHeader>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextDescriptionBlock"/> class by reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public TextDescriptionBlock(Stream stream) : base(new TextDescriptionHeader(stream), stream)
    {
    }

    internal TextDescriptionBlock(byte[] headerData, byte[] data) : base(new TextDescriptionHeader(headerData), data)
    {
    }
}