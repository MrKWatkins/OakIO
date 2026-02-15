using System.Text;

namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class BrowsePointBlock(Stream stream) : PzxBlock<BrowsePointHeader>(new BrowsePointHeader(stream), stream)
{
    public string Text => Encoding.ASCII.GetString(AsSpan());

    public override string ToString() => $"Browse: {Text}";
}