using System.Text;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A PZX block that marks a browse point in the tape.
/// </summary>
public sealed class BrowsePointBlock : PzxBlock<BrowsePointHeader>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="BrowsePointBlock" /> class from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public BrowsePointBlock(Stream stream) : base(new BrowsePointHeader(stream), stream)
    {
    }

    internal BrowsePointBlock(byte[] headerData, byte[] data) : base(new BrowsePointHeader(headerData), data)
    {
    }

    /// <summary>
    /// Gets the text of the browse point.
    /// </summary>
    public string Text => Encoding.ASCII.GetString(AsSpan());

    /// <inheritdoc />
    public override string ToString() => $"Browse: {Text}";
}