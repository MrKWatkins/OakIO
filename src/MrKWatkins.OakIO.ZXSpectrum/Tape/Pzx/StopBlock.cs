namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A PZX block that signals the tape should stop.
/// </summary>
public sealed class StopBlock : PzxBlock<StopHeader>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="StopBlock" /> class from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public StopBlock(Stream stream) : base(new StopHeader(stream), stream)
    {
    }

    internal StopBlock(byte[] headerData) : base(new StopHeader(headerData), [])
    {
    }

    /// <inheritdoc />
    public override string ToString() => Header.Only48k ? "Stop: 48k only" : "Stop: Always";
}