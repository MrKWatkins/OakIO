namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

public abstract class SnaFile : ZXSpectrumSnapshotFile
{
    private protected SnaFile(SnaHeader header)
        : base(SnaFormat.Instance)
    {
        Header = header;
    }

    public SnaHeader Header { get; }

    public override RegisterSnapshot Registers => Header.Registers;
}