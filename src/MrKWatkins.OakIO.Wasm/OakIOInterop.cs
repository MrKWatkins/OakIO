using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using MrKWatkins.OakIO.Commands;
using MrKWatkins.OakIO.Compression;

[assembly: SupportedOSPlatform("browser")]

namespace MrKWatkins.OakIO.Wasm;

// Task.Run is used to avoid blocking the calling thread.
public static partial class OakIOInterop
{
    [JSExport]
    public static async Task<string> GetInfo(string inputFilename, byte[] inputData) =>
        await Task.Run(() => InfoCommand.GetFileInfoJsonAsync(inputFilename, inputData)).ConfigureAwait(false);

    [JSExport]
    public static async Task<string> Convert(string inputFilename, byte[] inputData, string outputFilename, string compressionFormat) =>
        await Task.Run(async () =>
        {
            var format = Enum.Parse<CompressionFormat>(compressionFormat);
            var bytes = await ConvertCommand.ExecuteAsync(inputFilename, inputData, outputFilename, format).ConfigureAwait(false);
            return System.Convert.ToBase64String(bytes);
        }).ConfigureAwait(false);

    [JSExport]
    public static string GetCompressedFilename(string filename, string compressionFormat) =>
        IOFile.GetCompressedFilename(filename, Enum.Parse<CompressionFormat>(compressionFormat));
}