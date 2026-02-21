namespace MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;

public abstract class SnaSnapshotFile : SnapshotFile
{
    private protected SnaSnapshotFile(SnaSnapshotHeader header)
        : base(SnaSnapshotFormat.Instance)
    {
        Header = header;
    }

    public SnaSnapshotHeader Header { get; }

    public override RegisterSnapshot Registers => Header.Registers;
}
