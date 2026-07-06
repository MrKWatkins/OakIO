namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot;

/// <summary>
/// Base class for ZX Spectrum snapshot files.
/// </summary>
public abstract class ZXSpectrumSnapshotFile : ZXSpectrumFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZXSpectrumSnapshotFile" /> class.
    /// </summary>
    /// <param name="format">The snapshot format of the file.</param>
    protected ZXSpectrumSnapshotFile(ZXSpectrumSnapshotFormat format)
        : base(format)
    {
    }

    /// <summary>
    /// Gets the snapshot format.
    /// </summary>
    public new ZXSpectrumSnapshotFormat Format => (ZXSpectrumSnapshotFormat)base.Format;

    /// <summary>
    /// Gets the CPU register snapshot.
    /// </summary>
    public abstract RegisterSnapshot Registers { get; }
}