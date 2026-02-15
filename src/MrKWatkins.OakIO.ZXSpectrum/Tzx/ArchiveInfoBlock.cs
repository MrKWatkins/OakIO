using System.Text;

namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class ArchiveInfoBlock : TzxBlock<ArchiveInfoHeader>
{
    public ArchiveInfoBlock(Stream stream)
        : base(new ArchiveInfoHeader(stream), stream)
    {
        Entries = GetEntries(Header.NumberOfTextStrings, AsSpan());
    }

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