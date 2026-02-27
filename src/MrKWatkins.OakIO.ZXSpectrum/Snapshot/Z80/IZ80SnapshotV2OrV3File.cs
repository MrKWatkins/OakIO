namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Interface for V2 and V3 Z80 snapshot files that contain paged memory.
/// </summary>
public interface IZ80SnapshotV2OrV3File
{
    /// <summary>
    /// Gets the memory pages in the snapshot.
    /// </summary>
    IReadOnlyList<Page> Pages { get; }
}