using System.Text;
using System.Text.RegularExpressions;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The PZX file header block containing version information and metadata.
/// </summary>
public sealed class PzxHeaderBlock : PzxBlock<PzxHeader>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="PzxHeaderBlock" /> class from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public PzxHeaderBlock(Stream stream) : base(new PzxHeader(stream), stream)
    {
        Info = ReadInfos().ToList();
    }

    internal PzxHeaderBlock(byte[] headerData) : base(new PzxHeader(headerData), [])
    {
        Info = ReadInfos().ToList();
    }

    internal PzxHeaderBlock(byte[] headerData, byte[] data) : base(new PzxHeader(headerData), data)
    {
        Info = ReadInfos().ToList();
    }

    /// <summary>
    /// Gets the information entries from the header.
    /// </summary>
    public IReadOnlyList<Info> Info { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        var header = $"PZX {Header.MajorVersionNumber}.{Header.MinorVersionNumber}";

        return Info.Count > 0
            ? $"{header}{Environment.NewLine}{string.Join(Environment.NewLine, Info.Select(i => $"\t{Regex.Replace(i.ToString(), "[\r\n]+", $"{Environment.NewLine}\t\t")}"))}"
            : header;
    }

    private IEnumerable<Info> ReadInfos()
    {
        var type = "Title";
        var text = new StringBuilder();
        foreach (var @byte in Data)
        {
            if (@byte == 0)
            {
                if (type == null)
                {
                    type = text.ToString();
                }
                else
                {
                    yield return new Info(type, text.ToString());
                    type = null;
                }
                text.Clear();
                continue;
            }

            text.Append((char)@byte);
        }

        if (type != null && text.Length > 0)
        {
            yield return new Info(type, text.ToString());
        }
    }
}