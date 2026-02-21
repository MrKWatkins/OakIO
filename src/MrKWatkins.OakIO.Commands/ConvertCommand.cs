using MrKWatkins.OakIO.Wav;
using MrKWatkins.OakIO.ZXSpectrum;
using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;

namespace MrKWatkins.OakIO.Commands;

public sealed class ConvertCommand
{
    private static readonly IReadOnlyList<FileFormat> SupportedOutputFormats =
        [.. ZXSpectrumFile.TapeFormats, WavFormat.Instance];

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
        var outputFormat = GetOutputFormat(outputFilename);
        var outputFile = Convert(inputFile, outputFormat);
        outputFormat.Write(outputFile, outputStream);
    }

    [Pure]
    private static FileFormat GetOutputFormat(string outputFilename)
    {
        var extension = Path.GetExtension(outputFilename).TrimStart('.').ToLowerInvariant();
        return SupportedOutputFormats.FirstOrDefault(f => f.FileExtension == extension)
            ?? throw new NotSupportedException($"The output format \"{extension}\" is not supported.");
    }

    [Pure]
    private static IOFile Convert(IOFile inputFile, FileFormat outputFormat) =>
        (inputFile, outputFormat) switch
        {
            (TapFile tap, TzxFormat) => TapToTzxConverter.Instance.Convert(tap),
            (TapFile tap, PzxFormat) => TapToPzxConverter.Instance.Convert(tap),
            (TapFile tap, WavFormat) => new TapToWavConverter().Convert(tap),
            (TzxFile tzx, PzxFormat) => new TzxToPzxConverter().Convert(tzx),
            (TzxFile tzx, WavFormat) => new TzxToWavConverter().Convert(tzx),
            (PzxFile pzx, TzxFormat) => new PzxToTzxConverter().Convert(pzx),
            (PzxFile pzx, WavFormat) => new PzxToWavConverter().Convert(pzx),
            _ => throw new NotSupportedException($"Cannot convert from {inputFile.Format.Name} to {outputFormat.Name}.")
        };
}