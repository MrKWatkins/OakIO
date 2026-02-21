using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using MrKWatkins.OakIO.Commands;

[assembly: SupportedOSPlatform("browser")]

namespace MrKWatkins.OakIO.Wasm;

public static partial class OakIOInterop
{
    [JSExport]
    public static string GetInfo(string inputFilename, byte[] inputData) =>
        InfoCommand.Execute(inputFilename, inputData);

    [JSExport]
    public static byte[] Convert(string inputFilename, byte[] inputData, string outputFilename) =>
        ConvertCommand.Execute(inputFilename, inputData, outputFilename);
}