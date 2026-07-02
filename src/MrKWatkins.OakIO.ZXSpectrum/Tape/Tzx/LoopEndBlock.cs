namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that marks the end of a loop.
/// </summary>
public sealed class LoopEndBlock : TzxBlock<LoopEndHeader>
{
    internal LoopEndBlock(byte[] headerData) : base(new LoopEndHeader(headerData), [])
    {
    }
}