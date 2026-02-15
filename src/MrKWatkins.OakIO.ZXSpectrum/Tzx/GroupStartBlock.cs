namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class GroupStartBlock(Stream stream) : TzxTextBlock<GroupStartHeader>(new GroupStartHeader(stream), stream);