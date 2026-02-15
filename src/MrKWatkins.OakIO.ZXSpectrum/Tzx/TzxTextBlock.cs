using System.Text;

namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public abstract class TzxTextBlock<THeader> : TzxBlock<THeader>
    where THeader : TzxBlockHeader
{
    private protected TzxTextBlock(THeader header, Stream stream) : base(header, stream)
    {
    }

    public string Text => Encoding.ASCII.GetString(AsSpan());

    public override string ToString() => $"{Header}: {Text}";
}