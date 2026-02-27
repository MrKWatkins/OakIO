using System.IO.Compression;
using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// A version 1 Z80 snapshot file.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80V1File : Z80File<Z80V1Header>
{
    private readonly byte[] compressedData;

    internal Z80V1File(Z80V1Header header, byte[] compressedData) : base(header)
    {
        this.compressedData = compressedData;
    }

    /// <summary>
    /// Creates a new V1 Z80 snapshot file for a 48K Spectrum from the given memory.
    /// </summary>
    /// <param name="memory">The 64K memory contents.</param>
    /// <param name="compress"><c>true</c> to compress the data; <c>false</c> otherwise.</param>
    /// <returns>A new <see cref="Z80V1File" />.</returns>
    [Pure]
    public static Z80V1File Create48k(Span<byte> memory, bool compress = true)
    {
        var header = new Z80V1Header { DataIsCompressed = compress };
        var data = memory[16384..].ToArray();
        if (compress)
        {
            data = Compress(data);
        }

        return new Z80V1File(header, data);
    }

    [Pure]
    private static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using (var compressionStream = new Z80CompressionStream(compressedStream, CompressionMode.Compress, endMarker: false))
        {
            compressionStream.Write(data);
        }
        return compressedStream.ToArray();
    }

    /// <summary>
    /// Gets the compressed snapshot data.
    /// </summary>
    public ReadOnlySpan<byte> CompressedData => compressedData;

    /// <summary>
    /// Gets the uncompressed snapshot data.
    /// </summary>
    public ReadOnlySpan<byte> UncompressedData
    {
        get
        {
            if (!Header.DataIsCompressed)
            {
                return compressedData;
            }

            using var stream = new ReadOnlyListStream(compressedData);
            using var decompressed = new Z80CompressionStream(stream, CompressionMode.Decompress);
            return decompressed.ReadExactly(49152);
        }
    }

    /// <inheritdoc />
    public override bool TryLoadInto(Span<byte> memory)
    {
        UncompressedData.CopyTo(memory[16384..]);
        return true;
    }

    /// <inheritdoc />
    public override RegisterSnapshot Registers => Header.Registers;
}