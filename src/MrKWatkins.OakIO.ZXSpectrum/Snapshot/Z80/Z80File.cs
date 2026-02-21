namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

public abstract class Z80File : SnapshotFile
{
    private protected Z80File(Z80V1Header header)
        : base(Z80Format.Instance)
    {
        Header = header;
    }

    public Z80V1Header Header { get; }
}

public abstract class Z80File<THeader> : Z80File
    where THeader : Z80V1Header
{
    private protected Z80File(THeader header)
        : base(header)
    {
    }

    public new THeader Header => (THeader)base.Header;
}