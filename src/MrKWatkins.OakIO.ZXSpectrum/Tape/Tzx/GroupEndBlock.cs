namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that marks the end of a group of blocks.
/// </summary>
/// <param name="stream">The stream to read from.</param>
public sealed class GroupEndBlock(Stream stream) : TzxBlock<GroupEndHeader>(new GroupEndHeader(stream), stream);