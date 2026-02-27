namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that marks the end of a loop.
/// </summary>
/// <param name="stream">The stream to read from.</param>
public sealed class LoopEndBlock(Stream stream) : TzxBlock<LoopEndHeader>(new LoopEndHeader(stream), stream);