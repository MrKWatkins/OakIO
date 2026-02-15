namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

public abstract class Z80SnapshotV2OrV3File<THeader> : Z80SnapshotFile<THeader>, IZ80SnapshotV2OrV3File
    where THeader : Z80SnapshotV2Header
{
    private protected Z80SnapshotV2OrV3File(THeader header, [InstantHandle] IEnumerable<Page> pages)
        : base(header)
    {
        Pages = pages.ToArray();
        if (Pages.Count == 0)
        {
            throw new ArgumentException("Value is empty.", nameof(pages));
        }
    }

    public IReadOnlyList<Page> Pages { get; }

    public override RegisterSnapshot Registers => Header.Registers;

    public sealed override bool TryLoadInto(Span<byte> memory)
    {
        foreach (var page in Pages)
        {
            page.LoadInto(memory);
        }

        return true;
    }
}