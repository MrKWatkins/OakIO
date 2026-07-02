using System.IO.Compression;

namespace MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

/// <summary>
/// Helper methods for the zlib compression used by RZX snapshot and input recording blocks.
/// </summary>
internal static class ZLib
{
    [Pure]
    public static byte[] Decompress(byte[] compressedData, int? expectedLength = null)
    {
        if (expectedLength is { } expectedLengthValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(expectedLengthValue, nameof(expectedLength));
        }

        using var compressed = new MemoryStream(compressedData);
        using var zLib = new ZLibStream(compressed, CompressionMode.Decompress);
        using var decompressed = expectedLength is { } length ? new MemoryStream(length) : new MemoryStream();
        zLib.CopyTo(decompressed);

        var data = decompressed.ToArray();
        if (expectedLength is { } expected && data.Length != expected)
        {
            throw new InvalidDataException($"Compressed RZX data expanded to {data.Length} bytes instead of {expected}.");
        }

        return data;
    }
}