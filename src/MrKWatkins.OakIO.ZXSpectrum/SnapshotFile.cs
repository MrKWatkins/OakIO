namespace MrKWatkins.OakIO.ZXSpectrum;

public abstract class SnapshotFile : IOFile
{
    private protected SnapshotFile(SnapshotFormat format)
        : base(format)
    {
    }

    public new SnapshotFormat Format => (SnapshotFormat)base.Format;

    public abstract RegisterSnapshot Registers { get; }
}