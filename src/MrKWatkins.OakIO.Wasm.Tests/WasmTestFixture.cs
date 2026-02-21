using MrKWatkins.OakIO.Commands;
using MrKWatkins.OakIO.ZXSpectrum.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.Wasm.Tests;

public abstract class WasmTestFixture
{
    [Pure]
    protected static string GetInfo(string inputFilename, byte[] inputData) =>
        InfoCommand.Execute(inputFilename, inputData);

    [Pure]
    protected static byte[] Convert(string inputFilename, byte[] inputData, string outputFilename) =>
        ConvertCommand.Execute(inputFilename, inputData, outputFilename);

    [Pure]
    protected static byte[] CreateTapData()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        using var stream = new MemoryStream();
        TapFormat.Instance.Write(tap, stream);
        return stream.ToArray();
    }

    [Pure]
    protected static byte[] CreateTzxData()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        stream.WriteByte(0x10);
        stream.Write([0xE8, 0x03]);
        stream.Write([0x04, 0x00]);
        stream.Write([0xFF, 0x01, 0x02, 0x00]);
        return stream.ToArray();
    }

    [Pure]
    protected static byte[] CreatePzxData()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        return stream.ToArray();
    }

    [Pure]
    protected static byte[] CreateZ80Data()
    {
        var memory = new byte[48 * 1024];
        var snapshot = Z80SnapshotV1File.Create48k(memory);
        snapshot.Header.Registers.PC = 0x1000;
        using var stream = new MemoryStream();
        snapshot.Write(stream);
        return stream.ToArray();
    }
}