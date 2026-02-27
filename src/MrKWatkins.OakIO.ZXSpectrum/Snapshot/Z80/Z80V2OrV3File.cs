namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Base class for V2 and V3 Z80 snapshot files that contain paged memory.
/// </summary>
/// <typeparam name="THeader">The type of header.</typeparam>
public abstract class Z80V2OrV3File<THeader> : Z80File<THeader>, IZ80SnapshotV2OrV3File
    where THeader : Z80V2Header
{
    private protected Z80V2OrV3File(THeader header, [InstantHandle] IEnumerable<Page> pages)
        : base(header)
    {
        Pages = pages.ToArray();
        if (Pages.Count == 0)
        {
            throw new ArgumentException("Value is empty.", nameof(pages));
        }
    }

    /// <summary>
    /// Gets the memory pages in the snapshot.
    /// </summary>
    public IReadOnlyList<Page> Pages { get; }

    /// <inheritdoc />
    public override RegisterSnapshot Registers => Header.Registers;

    /// <inheritdoc />
    public sealed override bool TryLoadInto(Span<byte> memory)
    {
        foreach (var page in Pages)
        {
            page.LoadInto(memory);
        }

        return true;
    }
}