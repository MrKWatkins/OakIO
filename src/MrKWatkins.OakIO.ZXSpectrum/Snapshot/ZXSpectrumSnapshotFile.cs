namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot;

/// <summary>
/// Base class for ZX Spectrum snapshot files.
/// </summary>
/// <param name="format">The snapshot format of the file.</param>
public abstract class ZXSpectrumSnapshotFile(ZXSpectrumSnapshotFormat format) : ZXSpectrumFile(format)
{

    /// <summary>
    /// Gets the snapshot format.
    /// </summary>
    public new ZXSpectrumSnapshotFormat Format => (ZXSpectrumSnapshotFormat)base.Format;

    /// <summary>
    /// Gets the CPU register snapshot.
    /// </summary>
    public abstract RegisterSnapshot Registers { get; }
}