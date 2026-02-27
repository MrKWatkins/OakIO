namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing pure data with no pilot tone.
/// </summary>
public sealed class PureDataBlock : TzxBlock<PureDataHeader>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PureDataBlock"/> class by reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public PureDataBlock(Stream stream) : base(new PureDataHeader(stream), stream)
    {
    }

    internal PureDataBlock(byte[] headerData, byte[] data) : base(new PureDataHeader(headerData), data)
    {
    }
}