using MrKWatkins.OakIO.Compression;
using MrKWatkins.OakIO.ZXSpectrum;

namespace MrKWatkins.OakIO.Commands;

public static class ConvertCommand
{
    [Pure]
    public static byte[] Execute(string inputFilename, byte[] inputData, string outputFilename, CompressionFormat compressionFormat = CompressionFormat.None)
    {
        using var inputStream = new MemoryStream(inputData);
        using var outputStream = new MemoryStream();
        Execute(inputFilename, inputStream, outputFilename, outputStream, compressionFormat);
        return outputStream.ToArray();
    }

    public static void Execute(string inputFilename, Stream inputStream, string outputFilename, Stream outputStream, CompressionFormat compressionFormat = CompressionFormat.None)
    {
        var inputFile = ZXSpectrumFileFormat.Load(inputFilename, inputStream);
        var outputFormat = GetOutputFormat(inputFile.Format, outputFilename);
        var outputFile = IOFileConversion.Convert(inputFile, outputFormat);
        outputFile.Write(outputStream, outputFilename, compressionFormat);
    }

    [Pure]
    public static async Task<byte[]> ExecuteAsync(string inputFilename, byte[] inputData, string outputFilename, CompressionFormat compressionFormat = CompressionFormat.None, CancellationToken cancellationToken = default)
    {
        using var inputStream = new MemoryStream(inputData);
        using var outputStream = new MemoryStream();
        await ExecuteAsync(inputFilename, inputStream, outputFilename, outputStream, compressionFormat, cancellationToken).ConfigureAwait(false);
        return outputStream.ToArray();
    }

    public static async Task ExecuteAsync(string inputFilename, Stream inputStream, string outputFilename, Stream outputStream, CompressionFormat compressionFormat = CompressionFormat.None, CancellationToken cancellationToken = default)
    {
        var inputFile = await ZXSpectrumFileFormat.LoadAsync(inputFilename, inputStream, cancellationToken).ConfigureAwait(false);
        var outputFormat = GetOutputFormat(inputFile.Format, outputFilename);
        var outputFile = IOFileConversion.Convert(inputFile, outputFormat);
        await outputFile.WriteAsync(outputStream, outputFilename, compressionFormat, cancellationToken).ConfigureAwait(false);
    }

    [Pure]
    private static IOFileFormat GetOutputFormat(IOFileFormat inputFormat, string outputFilename)
    {
        var extension = Path.GetExtension(outputFilename).TrimStart('.').ToLowerInvariant();
        return IOFileConversion.GetSupportedConversionFormats(inputFormat).FirstOrDefault(f => f.FileExtension == extension)
               ?? throw new NotSupportedException($"The output format \"{extension}\" is not supported.");
    }
}