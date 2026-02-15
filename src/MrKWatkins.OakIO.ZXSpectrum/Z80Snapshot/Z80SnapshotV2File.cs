namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80SnapshotV2File : Z80SnapshotV2OrV3File<Z80SnapshotV2Header>
{
    internal Z80SnapshotV2File(Z80SnapshotV2Header header, [InstantHandle] IEnumerable<Page> pages) : base(header, pages)
    {
    }

    [Pure]
    public static Z80SnapshotV2File Create48k(Span<byte> memory, bool compress = true) =>
        new(new Z80SnapshotV2Header(), Page.Create48k(memory, compress));
}