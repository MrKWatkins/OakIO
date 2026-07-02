namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that marks the start of a group of blocks with a name.
/// </summary>
public sealed class GroupStartBlock : TzxTextBlock<GroupStartHeader>
{
    internal GroupStartBlock(byte[] headerData, byte[] data) : base(new GroupStartHeader(headerData), data)
    {
    }
}