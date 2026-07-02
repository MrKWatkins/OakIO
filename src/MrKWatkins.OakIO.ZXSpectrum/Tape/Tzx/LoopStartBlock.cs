namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that marks the start of a loop with a specified number of repetitions.
/// </summary>
public sealed class LoopStartBlock : TzxBlock<LoopStartHeader>
{
    internal LoopStartBlock(byte[] headerData) : base(new LoopStartHeader(headerData), [])
    {
    }
}