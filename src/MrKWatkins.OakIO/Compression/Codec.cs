using System.Collections.Frozen;

namespace MrKWatkins.OakIO.Compression;

internal abstract class Codec
{
    private static readonly FrozenDictionary<string, CompressionFormat> CompressionFormatByFileExtensionWithoutPeriod = new Dictionary<string, CompressionFormat>(StringComparer.OrdinalIgnoreCase)
    {
        { "br", CompressionFormat.Brotli },
        { "gz", CompressionFormat.GZip },
        { "zip", CompressionFormat.Zip },
        { "zst", CompressionFormat.Zstandard },
    }.ToFrozenDictionary();

    [MustUseReturnValue]
    public static IOFile Load([PathReference] string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats)
        => ValidateSupportedFormatsGetCodecAndForLoad(path, supportedFormats).Read(path, stream, supportedFormats);

    [MustUseReturnValue]
    public static Task<IOFile> LoadAsync([PathReference] string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats, CancellationToken cancellationToken)
        => ValidateSupportedFormatsGetCodecAndForLoad(path, supportedFormats).ReadAsync(path, stream, supportedFormats, cancellationToken);

    [Pure]
    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private static Codec ValidateSupportedFormatsGetCodecAndForLoad([PathReference] string path, IReadOnlyList<IOFileFormat> supportedFormats)
    {
        if (supportedFormats.Count == 0)
        {
            throw new ArgumentException("At least one supported format must be provided.", nameof(supportedFormats));
        }

        var extensionWithPeriod = Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(extensionWithPeriod))
        {
            throw new NotSupportedException("Files without extensions are not supported.");
        }

        return GetCodec(extensionWithPeriod[1..]);
    }

    [MustUseReturnValue]
    protected abstract IOFile Read([PathReference] string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats);

    [MustUseReturnValue]
    protected abstract Task<IOFile> ReadAsync([PathReference] string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats, CancellationToken cancellationToken);

    [Pure]
    protected static bool TryGetFileFormat(string filename, IReadOnlyList<IOFileFormat> supportedFormats, [MaybeNullWhen(false)] out IOFileFormat fileFormat)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            fileFormat = null;
            return false;
        }

        extension = extension[1..];
        fileFormat = supportedFormats.FirstOrDefault(f => f.FileExtension == extension);
        return fileFormat != null;
    }

    [Pure]
    protected static IOFileFormat GetFileFormat(string filename, IReadOnlyList<IOFileFormat> supportedFormats)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new NotSupportedException("Files without extensions are not supported.");
        }

        extension = extension[1..];
        var fileFormat = supportedFormats.FirstOrDefault(f => f.FileExtension == extension);
        return fileFormat ?? throw new NotSupportedException($"Files with the extension '{extension}' are not supported.");
    }

    public static string Save(IOFile file, [PathReference] string outputDirectory, string filename, CompressionFormat format)
    {
        var (codec, filenameWithFormatExtension, compressedPath) = GetCodecAndCompressedPathForSave(file, outputDirectory, filename, format);
        using var fileStream = File.Create(compressedPath);
        codec.Write(file, fileStream, filenameWithFormatExtension);
        return compressedPath;
    }

    public static async Task<string> SaveAsync(IOFile file, [PathReference] string outputDirectory, string filename, CompressionFormat format, CancellationToken cancellationToken)
    {
        var (codec, filenameWithFormatExtension, compressedPath) = GetCodecAndCompressedPathForSave(file, outputDirectory, filename, format);
        var fileStream = File.Create(compressedPath);
        await using (fileStream.ConfigureAwait(false))
        {
            await codec.WriteAsync(file, fileStream, filenameWithFormatExtension, cancellationToken).ConfigureAwait(false);
            return compressedPath;
        }
    }

    [Pure]
    private static (Codec Codec, string FilenameWithFormatExtension, string CompressedPath) GetCodecAndCompressedPathForSave(IOFile file, [PathReference] string outputDirectory, string filename, CompressionFormat format)
    {
        if (!Directory.Exists(outputDirectory))
        {
            throw new DirectoryNotFoundException($"The output directory '{outputDirectory}' does not exist.");
        }

        var codec = GetCodec(format);

        var filenameWithFormatExtension = file.Format.GetFilename(filename);
        var compressedFilename = codec.GetCompressedFilename(filenameWithFormatExtension);
        var compressedPath = Path.Combine(outputDirectory, compressedFilename);
        return (codec, filenameWithFormatExtension, compressedPath);
    }

    [Pure]
    protected abstract string GetCompressedFilename(string filenameWithFormatExtension);

    protected abstract void Write(IOFile file, Stream stream, string filenameWithFormatExtension);

    protected abstract Task WriteAsync(IOFile file, Stream stream, string filenameWithFormatExtension, CancellationToken cancellationToken);

    [Pure]
    private static Codec GetCodec(CompressionFormat format) =>
        format switch
        {
            CompressionFormat.None => NoCompressionCodec.Instance,
            CompressionFormat.Brotli => BrotliCodec.Instance,
            CompressionFormat.GZip => GZipCodec.Instance,
            CompressionFormat.Zip => ZipCodec.Instance,
            CompressionFormat.Zstandard => ZstandardCodec.Instance,
            _ => throw new NotSupportedException($"Compression format {format} is not supported.")
        };

    [Pure]
    private static Codec GetCodec(string fileExtensionWithoutPeriod) =>
        GetCodec(CompressionFormatByFileExtensionWithoutPeriod.GetValueOrDefault(fileExtensionWithoutPeriod, CompressionFormat.None));
}