namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

/// <summary>
/// Base class for SNA snapshot files.
/// </summary>
public abstract class SnaFile : ZXSpectrumSnapshotFile
{
    private protected SnaFile(SnaHeader header)
        : base(SnaFormat.Instance)
    {
        Header = header;
    }

    /// <summary>
    /// Gets the SNA header.
    /// </summary>
    public SnaHeader Header { get; }

    /// <inheritdoc />
    public override RegisterSnapshot Registers => Header.Registers;
}