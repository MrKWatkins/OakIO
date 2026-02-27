namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that generates a single tone consisting of identical pulses.
/// </summary>
public sealed class PureToneBlock : TzxBlock<PureToneHeader>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PureToneBlock"/> class by reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public PureToneBlock(Stream stream) : base(new PureToneHeader(stream), stream)
    {
    }

    internal PureToneBlock(byte[] headerData) : base(new PureToneHeader(headerData), [])
    {
    }
}