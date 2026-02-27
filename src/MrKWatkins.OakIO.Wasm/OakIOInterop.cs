using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using MrKWatkins.OakIO.Commands;

[assembly: SupportedOSPlatform("browser")]

namespace MrKWatkins.OakIO.Wasm;

public static partial class OakIOInterop
{
    [JSExport]
    public static async Task<string> GetInfo(string inputFilename, byte[] inputData) =>
        await Task.Run(() => InfoCommand.GetFileInfoJson(inputFilename, inputData)).ConfigureAwait(false);

    [JSExport]
    public static async Task<string> Convert(string inputFilename, byte[] inputData, string outputFilename) =>
        await Task.Run(() => System.Convert.ToBase64String(ConvertCommand.Execute(inputFilename, inputData, outputFilename))).ConfigureAwait(false);
}