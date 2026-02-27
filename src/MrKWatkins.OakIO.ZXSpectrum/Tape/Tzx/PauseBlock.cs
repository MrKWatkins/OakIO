namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block representing a pause or silence in the tape.
/// </summary>
public sealed class PauseBlock : TzxBlock<PauseHeader>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PauseBlock"/> class by reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public PauseBlock(Stream stream) : base(new PauseHeader(stream), stream)
    {
    }

    internal PauseBlock(byte[] headerData) : base(new PauseHeader(headerData), [])
    {
    }
}