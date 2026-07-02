namespace MrKWatkins.OakIO.Compression;

/// <summary>
/// Specifies the compression algorithm to use when writing a file.
/// </summary>
public enum CompressionFormat
{
    /// <summary>
    /// Specifies that no compression should be used when writing the file.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies that the file should be compressed using the ZIP algorithm when writing. A single file archive will be produced.
    /// </summary>
    Zip = 1,

    /// <summary>
    /// Specifies that the file should be compressed using the GZip algorithm when writing.
    /// </summary>
    GZip = 2,

    /// <summary>
    /// Specifies that the file should be compressed using the Brotli algorithm when writing.
    /// </summary>
    Brotli = 3,

    /// <summary>
    /// Specifies that the file should be compressed using the Zstandard algorithm when writing.
    /// </summary>
    Zstandard = 4
}