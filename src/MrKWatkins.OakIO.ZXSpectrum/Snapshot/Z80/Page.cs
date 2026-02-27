using System.IO.Compression;
using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// A memory page block in a V2 or V3 Z80 snapshot file.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Page : Block<PageHeader>
{
    internal Page(PageHeader header, int length, Stream data)
        : base(header, length, data)
    {
    }

    internal Page(PageHeader header, byte[] data)
        : base(header, data)
    {
    }

    /// <summary>
    /// Gets the uncompressed data for this page.
    /// </summary>
    public IReadOnlyList<byte> UncompressedData
    {
        get
        {
            if (Header.CompressedLength == 0xFFFF)
            {
                return Data;
            }

            using var stream = new ReadOnlyListStream(Data);
            using var decompressed = new Z80CompressionStream(stream, CompressionMode.Decompress, endMarker: false);
            return decompressed.ReadExactly(16384);
        }
    }

    /// <inheritdoc />
    public override bool TryLoadInto(Span<byte> memory)
    {
        UncompressedData.CopyTo(memory[Header.Location..]);
        return true;
    }

    /// <summary>
    /// Creates the three memory pages for a 48K Spectrum from the given memory.
    /// </summary>
    /// <param name="memory">The 64K memory contents.</param>
    /// <param name="compress"><c>true</c> to compress the page data; <c>false</c> otherwise.</param>
    /// <returns>A list of three pages covering the 48K memory.</returns>
    [Pure]
    public static IReadOnlyList<Page> Create48k(Span<byte> memory, bool compress = true) =>
    [
        Create48kPage(memory, 4, compress),
        Create48kPage(memory, 5, compress),
        Create48kPage(memory, 8, compress)
    ];

    [Pure]
    private static Page Create48kPage(Span<byte> memory, byte pageNumber, bool compress) =>
        compress
            ? CreateCompressed48kPage(memory, pageNumber)
            : CreateUncompressed48kPage(memory, pageNumber);

    [Pure]
    private static Page CreateCompressed48kPage(Span<byte> memory, byte pageNumber)
    {
        var dataLocation = PageHeader.GetLocation(HardwareMode.Spectrum48, pageNumber);
        var data = Compress(memory.Slice(dataLocation, 16384).ToArray());

        return new Page(new PageHeader(HardwareMode.Spectrum48, (ushort)data.Length, pageNumber), data);
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

    [Pure]
    private static Page CreateUncompressed48kPage(Span<byte> memory, byte pageNumber)
    {
        var dataLocation = PageHeader.GetLocation(HardwareMode.Spectrum48, pageNumber);
        var data = memory.Slice(dataLocation, 16384).ToArray();

        return new Page(new PageHeader(HardwareMode.Spectrum48, 0xFFFF, pageNumber), data);
    }
}