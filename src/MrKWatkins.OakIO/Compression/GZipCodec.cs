using System.IO.Compression;

namespace MrKWatkins.OakIO.Compression;

internal sealed class GZipCodec : CompressionStreamCodec
{
    public static readonly GZipCodec Instance = new();

    private GZipCodec()
    {
    }

    protected override string FileExtension => "gz";

    protected override Stream CreateDecompressionStream(Stream stream) => new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);

    protected override Stream CreateCompressionStream(Stream stream) => new GZipStream(stream, CompressionLevel.SmallestSize, leaveOpen: true);
}