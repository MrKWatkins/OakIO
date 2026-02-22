namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot;

public abstract class ZXSpectrumSnapshotFile : IOFile
{
    private protected ZXSpectrumSnapshotFile(ZXSpectrumSnapshotFormat format)
        : base(format)
    {
    }

    public new ZXSpectrumSnapshotFormat Format => (ZXSpectrumSnapshotFormat)base.Format;

    public abstract RegisterSnapshot Registers { get; }
}