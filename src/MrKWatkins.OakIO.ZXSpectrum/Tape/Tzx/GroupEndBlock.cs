namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class GroupEndBlock(Stream stream) : TzxBlock<GroupEndHeader>(new GroupEndHeader(stream), stream);