namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80SnapshotV3File : Z80SnapshotV2OrV3File<Z80SnapshotV3Header>
{
    internal Z80SnapshotV3File(Z80SnapshotV3Header header, [InstantHandle] IEnumerable<Page> pages) : base(header, pages)
    {
    }

    [Pure]
    public static Z80SnapshotV3File Create48k(Span<byte> memory, bool compress = true) =>
        new(new Z80SnapshotV3Header(), Page.Create48k(memory, compress));
}