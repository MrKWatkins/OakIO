using System.Text;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Base class for TZX blocks that contain ASCII text data.
/// </summary>
/// <typeparam name="THeader">The type of the block header.</typeparam>
public abstract class TzxTextBlock<THeader> : TzxBlock<THeader>
    where THeader : TzxBlockHeader
{
    private protected TzxTextBlock(THeader header, Stream stream) : base(header, stream)
    {
    }

    private protected TzxTextBlock(THeader header, byte[] data) : base(header, data)
    {
    }

    /// <summary>
    /// Gets the ASCII text content of this block.
    /// </summary>
    public string Text => Encoding.ASCII.GetString(AsSpan());

    /// <inheritdoc />
    public override string ToString() => $"{Header}: {Text}";
}