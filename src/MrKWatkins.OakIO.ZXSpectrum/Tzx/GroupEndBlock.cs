namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class GroupEndBlock(Stream stream) : TzxBlock<GroupEndHeader>(new GroupEndHeader(stream), stream);