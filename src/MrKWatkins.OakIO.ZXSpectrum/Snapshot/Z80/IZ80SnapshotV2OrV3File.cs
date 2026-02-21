namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

public interface IZ80SnapshotV2OrV3File
{
    IReadOnlyList<Page> Pages { get; }
}