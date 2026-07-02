using ZstdSharp;

namespace MrKWatkins.OakIO.Compression;

// TODO: Replace with framework implementation when upgrading to .NET 11.
internal sealed class ZstandardCodec : CompressionStreamCodec
{
    public static readonly ZstandardCodec Instance = new();

    private ZstandardCodec()
    {
    }

    protected override string FileExtension => "zst";

    protected override Stream CreateDecompressionStream(Stream stream) => new DecompressionStream(stream, leaveOpen: true);

    protected override Stream CreateCompressionStream(Stream stream) => new CompressionStream(stream, level: Compressor.MaxCompressionLevel, leaveOpen: true);
}