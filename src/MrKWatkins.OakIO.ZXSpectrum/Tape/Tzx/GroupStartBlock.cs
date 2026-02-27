namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that marks the start of a group of blocks with a name.
/// </summary>
/// <param name="stream">The stream to read from.</param>
public sealed class GroupStartBlock(Stream stream) : TzxTextBlock<GroupStartHeader>(new GroupStartHeader(stream), stream);