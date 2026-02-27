namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that marks the start of a loop with a specified number of repetitions.
/// </summary>
/// <param name="stream">The stream to read from.</param>
public sealed class LoopStartBlock(Stream stream) : TzxBlock<LoopStartHeader>(new LoopStartHeader(stream), stream);