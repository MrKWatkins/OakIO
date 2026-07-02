namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// Flags for an <see cref="RzxSnapshotBlock" />.
/// </summary>
#pragma warning disable CA1028, CA1711
[Flags]
public enum RzxSnapshotFlags : uint
{
    /// <summary>
    /// No flags set; the block contains an uncompressed, embedded snapshot.
    /// </summary>
    None = 0,

    /// <summary>
    /// The block contains a snapshot descriptor referencing external data rather than the snapshot image itself.
    /// </summary>
    ExternalData = 1,

    /// <summary>
    /// The snapshot data is compressed with zlib.
    /// </summary>
    Compressed = 2
}
#pragma warning restore CA1028, CA1711