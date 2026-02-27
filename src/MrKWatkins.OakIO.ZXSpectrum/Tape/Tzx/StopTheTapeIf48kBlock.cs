namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that signals the tape should stop if the machine is a 48K ZX Spectrum.
/// </summary>
public sealed class StopTheTapeIf48KBlock : TzxBlock<StopTheTapeIf48KHeader>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StopTheTapeIf48KBlock"/> class by reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public StopTheTapeIf48KBlock(Stream stream) : base(new StopTheTapeIf48KHeader(stream), stream)
    {
    }

    internal StopTheTapeIf48KBlock(byte[] headerData) : base(new StopTheTapeIf48KHeader(headerData), [])
    {
    }
}