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

    /// <summary>
    /// Gets the hardware mode of the snapshot.
    /// </summary>
    HardwareMode HardwareMode { get; }

    /// <summary>
    /// Gets the last value written to port $7FFD.
    /// </summary>
    byte Port7FFD { get; }

    /// <summary>
    /// Gets an uncompressed physical 128K RAM bank, numbered 0–7.
    /// </summary>
    [Pure]
    IReadOnlyList<byte> GetBank(int bankNumber);
}