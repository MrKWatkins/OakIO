namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that marks the end of a group of blocks.
/// </summary>
public sealed class GroupEndBlock : TzxBlock<GroupEndHeader>
{
    internal GroupEndBlock(byte[] headerData) : base(new GroupEndHeader(headerData), [])
    {
    }
}