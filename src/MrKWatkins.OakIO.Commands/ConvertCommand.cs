using MrKWatkins.OakIO.ZXSpectrum;

namespace MrKWatkins.OakIO.Commands;

public static class ConvertCommand
{
    [Pure]
    public static byte[] Execute(string inputFilename, byte[] inputData, string outputFilename)
    {
        using var inputStream = new MemoryStream(inputData);
        using var outputStream = new MemoryStream();
        Execute(inputFilename, inputStream, outputFilename, outputStream);
        return outputStream.ToArray();
    }

    public static void Execute(string inputFilename, Stream inputStream, string outputFilename, Stream outputStream)
    {
        var inputFile = ZXSpectrumFile.Read(inputFilename, inputStream);
        var outputFormat = GetOutputFormat(inputFile.Format, outputFilename);
        var outputFile = IOFileConversion.Convert(inputFile, outputFormat);
        outputFormat.Write(outputFile, outputStream);
    }

    [Pure]
    private static IOFileFormat GetOutputFormat(IOFileFormat inputFormat, string outputFilename)
    {
        var extension = Path.GetExtension(outputFilename).TrimStart('.').ToLowerInvariant();
        return IOFileConversion.GetSupportedConversionFormats(inputFormat).FirstOrDefault(f => f.FileExtension == extension)
               ?? throw new NotSupportedException($"The output format \"{extension}\" is not supported.");
    }
}