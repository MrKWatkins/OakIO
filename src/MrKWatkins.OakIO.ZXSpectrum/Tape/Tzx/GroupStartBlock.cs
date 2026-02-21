namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class GroupStartBlock(Stream stream) : TzxTextBlock<GroupStartHeader>(new GroupStartHeader(stream), stream);