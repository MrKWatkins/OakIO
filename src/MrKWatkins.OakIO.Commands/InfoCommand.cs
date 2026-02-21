using MrKWatkins.OakIO.ZXSpectrum;
using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;

namespace MrKWatkins.OakIO.Commands;

public sealed class InfoCommand
{
    [Pure]
    public static string Execute(string inputFilename, byte[] inputData)
    {
        using var inputStream = new MemoryStream(inputData);
        using var output = new StringWriter();
        Execute(inputFilename, inputStream, output);
        return output.ToString();
    }

    public static void Execute(string inputFilename, Stream inputStream, TextWriter output)
    {
        var file = ZXSpectrumFile.Read(inputFilename, inputStream);
        output.WriteLine($"Format: {file.Format.Name}");
        switch (file)
        {
            case TapFile tapFile:
                WriteBlocks(output, tapFile.Blocks);
                break;
            case TzxFile tzxFile:
                WriteBlocks(output, tzxFile.Blocks);
                break;
            case PzxFile pzxFile:
                WriteBlocks(output, pzxFile.Blocks);
                break;
            case SnapshotFile snapshot:
                WriteRegisters(output, snapshot.Registers);
                break;
        }
    }

    private static void WriteBlocks<T>(TextWriter output, IReadOnlyList<T> blocks)
    {
        output.WriteLine($"Blocks: {blocks.Count}");
        foreach (var (block, index) in blocks.Select((b, i) => (b, i + 1)))
        {
            output.WriteLine($"  {index}: {block}");
        }
    }

    private static void WriteRegisters(TextWriter output, RegisterSnapshot registers)
    {
        output.WriteLine($"AF: 0x{registers.AF:X4}");
        output.WriteLine($"BC: 0x{registers.BC:X4}");
        output.WriteLine($"DE: 0x{registers.DE:X4}");
        output.WriteLine($"HL: 0x{registers.HL:X4}");
        output.WriteLine($"IX: 0x{registers.IX:X4}");
        output.WriteLine($"IY: 0x{registers.IY:X4}");
        output.WriteLine($"PC: 0x{registers.PC:X4}");
        output.WriteLine($"SP: 0x{registers.SP:X4}");
        output.WriteLine($"IR: 0x{registers.IR:X4}");
    }
}