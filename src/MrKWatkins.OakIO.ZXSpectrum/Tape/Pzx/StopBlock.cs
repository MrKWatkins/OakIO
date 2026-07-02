namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A PZX block that signals the tape should stop.
/// </summary>
public sealed class StopBlock : PzxBlock<StopHeader>
{
    internal StopBlock(byte[] headerData) : base(new StopHeader(headerData), [])
    {
    }

    /// <inheritdoc />
    [Pure]
    public override string ToString() => Header.Only48k ? "Stop: 48k only" : "Stop: Always";
}