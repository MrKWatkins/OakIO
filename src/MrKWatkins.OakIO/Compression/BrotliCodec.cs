using System.IO.Compression;

namespace MrKWatkins.OakIO.Compression;

internal sealed class BrotliCodec : CompressionStreamCodec
{
    public static readonly BrotliCodec Instance = new();

    private BrotliCodec()
    {
    }

    protected override string FileExtension => "br";

    protected override Stream CreateDecompressionStream(Stream stream) => new BrotliStream(stream, CompressionMode.Decompress, leaveOpen: true);

    protected override Stream CreateCompressionStream(Stream stream) => new BrotliStream(stream, CompressionLevel.SmallestSize, leaveOpen: true);
}