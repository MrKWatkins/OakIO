using System.Text;

namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class BrowsePointBlock : PzxBlock<BrowsePointHeader>
{
    public BrowsePointBlock(Stream stream) : base(new BrowsePointHeader(stream), stream)
    {
    }

    internal BrowsePointBlock(byte[] headerData, byte[] data) : base(new BrowsePointHeader(headerData), data)
    {
    }

    public string Text => Encoding.ASCII.GetString(AsSpan());

    public override string ToString() => $"Browse: {Text}";
}