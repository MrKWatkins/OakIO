using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using MrKWatkins.OakIO.Commands;

[assembly: SupportedOSPlatform("browser")]

namespace MrKWatkins.OakIO.Wasm;

public static partial class OakIOInterop
{
    [JSExport]
    public static string GetInfo(string inputFilename, byte[] inputData)
    {
        using var inputStream = new MemoryStream(inputData);
        using var output = new StringWriter();
        InfoCommand.Execute(inputFilename, inputStream, output);
        return output.ToString();
    }

    [JSExport]
    public static byte[] Convert(string inputFilename, byte[] inputData, string outputFilename)
    {
        using var inputStream = new MemoryStream(inputData);
        using var outputStream = new MemoryStream();
        ConvertCommand.Execute(inputFilename, inputStream, outputFilename, outputStream);
        return outputStream.ToArray();
    }
}
