namespace MrKWatkins.OakIO.Compression;

internal abstract class CompressionStreamCodec : Codec
{
    protected override string GetCompressedFilename(string filenameWithFormatExtension) => $"{filenameWithFormatExtension}.{FileExtension}";

    protected abstract string FileExtension { get; }

    protected sealed override IOFile Read(string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats)
    {
        var pathWithoutCompressionExtension = StripCompressionExtension(path);
        var format = GetFileFormat(pathWithoutCompressionExtension, supportedFormats);

        using var compressionStream = CreateDecompressionStream(stream);
        return format.Read(compressionStream);
    }

    protected sealed override async Task<IOFile> ReadAsync(string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats, CancellationToken cancellationToken)
    {
        var pathWithoutCompressionExtension = StripCompressionExtension(path);
        var format = GetFileFormat(pathWithoutCompressionExtension, supportedFormats);

        var compressionStream = CreateDecompressionStream(stream);
        await using (compressionStream.ConfigureAwait(false))
        {
            return await format.ReadAsync(compressionStream, cancellationToken).ConfigureAwait(false);
        }
    }

    [Pure]
    private string StripCompressionExtension(string filenameWithFormatExtension) => filenameWithFormatExtension[..^(FileExtension.Length + 1)];

    [MustDisposeResource]
    protected abstract Stream CreateDecompressionStream(Stream stream);

    protected sealed override void Write(IOFile file, Stream stream, string filenameWithFormatExtension)
    {
        using var compressionStream = CreateCompressionStream(stream);
        file.Write(compressionStream);
    }

    protected sealed override async Task WriteAsync(IOFile file, Stream stream, string filenameWithFormatExtension, CancellationToken cancellationToken)
    {
        var compressionStream = CreateCompressionStream(stream);
        await using (compressionStream.ConfigureAwait(false))
        {
            await file.WriteAsync(compressionStream, cancellationToken).ConfigureAwait(false);
        }
    }

    [MustDisposeResource]
    protected abstract Stream CreateCompressionStream(Stream stream);
}