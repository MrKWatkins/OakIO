namespace MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

public interface IZ80SnapshotV2OrV3File
{
    IReadOnlyList<Page> Pages { get; }
}