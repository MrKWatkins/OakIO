namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

public abstract class Z80SnapshotFile : SnapshotFile
{
    private protected Z80SnapshotFile(Z80SnapshotV1Header header)
        : base(Z80SnapshotFormat.Instance)
    {
        Header = header;
    }

    public Z80SnapshotV1Header Header { get; }
}

public abstract class Z80SnapshotFile<THeader> : Z80SnapshotFile
    where THeader : Z80SnapshotV1Header
{
    private protected Z80SnapshotFile(THeader header)
        : base(header)
    {
    }

    public new THeader Header => (THeader)base.Header;
}