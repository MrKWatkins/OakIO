namespace MrKWatkins.OakIO.Compression;

internal sealed class NoCompressionCodec : Codec
{
    public static readonly NoCompressionCodec Instance = new();

    private NoCompressionCodec()
    {
    }

    protected override IOFile Read([PathReference] string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats) => GetFileFormat(path, supportedFormats).Read(stream);

    protected override Task<IOFile> ReadAsync(string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats, CancellationToken cancellationToken)
        => GetFileFormat(path, supportedFormats).ReadAsync(stream, cancellationToken);

    protected override string GetCompressedFilename(string filenameWithFormatExtension) => filenameWithFormatExtension;

    protected override void Write(IOFile file, Stream stream, string filenameWithFormatExtension) => file.Write(stream);

    protected override Task WriteAsync(IOFile file, Stream stream, string filenameWithFormatExtension, CancellationToken cancellationToken) => file.WriteAsync(stream, cancellationToken);
}