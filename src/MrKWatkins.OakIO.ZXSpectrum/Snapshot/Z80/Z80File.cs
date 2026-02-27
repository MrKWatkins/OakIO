namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Base class for Z80 snapshot files.
/// </summary>
public abstract class Z80File : ZXSpectrumSnapshotFile
{
    private protected Z80File(Z80V1Header header)
        : base(Z80Format.Instance)
    {
        Header = header;
    }

    /// <summary>
    /// Gets the V1 header for this Z80 file.
    /// </summary>
    public Z80V1Header Header { get; }
}

/// <summary>
/// Base class for Z80 snapshot files with a strongly-typed header.
/// </summary>
/// <typeparam name="THeader">The type of header.</typeparam>
public abstract class Z80File<THeader> : Z80File
    where THeader : Z80V1Header
{
    private protected Z80File(THeader header)
        : base(header)
    {
    }

    /// <summary>
    /// Gets the strongly-typed header for this Z80 file.
    /// </summary>
    public new THeader Header => (THeader)base.Header;
}