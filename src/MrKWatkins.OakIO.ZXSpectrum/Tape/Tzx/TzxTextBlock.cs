using System.Text;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public abstract class TzxTextBlock<THeader> : TzxBlock<THeader>
    where THeader : TzxBlockHeader
{
    private protected TzxTextBlock(THeader header, Stream stream) : base(header, stream)
    {
    }

    private protected TzxTextBlock(THeader header, byte[] data) : base(header, data)
    {
    }

    public string Text => Encoding.ASCII.GetString(AsSpan());

    public override string ToString() => $"{Header}: {Text}";
}