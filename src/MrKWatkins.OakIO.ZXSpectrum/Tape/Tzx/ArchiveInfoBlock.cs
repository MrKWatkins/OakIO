using System.Text;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX archive info block containing metadata about the tape.
/// </summary>
public sealed class ArchiveInfoBlock : TzxBlock<ArchiveInfoHeader>
{
    internal ArchiveInfoBlock(byte[] headerData, byte[] data)
        : base(new ArchiveInfoHeader(headerData), data)
    {
        Entries = GetEntries(Header.NumberOfTextStrings, AsSpan());
    }

    /// <summary>
    /// Gets the archive info entries in this block.
    /// </summary>
    public IReadOnlyList<ArchiveInfoEntry> Entries { get; }

    [Pure]
    private static List<ArchiveInfoEntry> GetEntries(int numberOfEntries, ReadOnlySpan<byte> bytes)
    {
        var entries = new List<ArchiveInfoEntry>(numberOfEntries);
        var index = 0;
        while (index < bytes.Length)
        {
            var type = (ArchiveInfoType)bytes[index];

            index++;
            var length = bytes[index];

            index++;
            var text = Encoding.ASCII.GetString(bytes.Slice(index, length));

            entries.Add(new ArchiveInfoEntry(type, text));
            index += length;
        }
        return entries;
    }
}