namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// Flags for an <see cref="RzxInputRecordingBlock" />.
/// </summary>
#pragma warning disable CA1028, CA1711
[Flags]
public enum RzxInputRecordingFlags : uint
{
    /// <summary>
    /// No flags set; the frames are unprotected and uncompressed.
    /// </summary>
    None = 0,

    /// <summary>
    /// The frames are encrypted with an x-key. Not supported.
    /// </summary>
    Protected = 1,

    /// <summary>
    /// The frame data is compressed with zlib.
    /// </summary>
    Compressed = 2
}
#pragma warning restore CA1028, CA1711