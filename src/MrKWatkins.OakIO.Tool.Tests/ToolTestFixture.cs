using MrKWatkins.OakIO.Testing;
using MrKWatkins.OakIO.ZXSpectrum.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.Tool.Tests;

public abstract class ToolTestFixture
{
    [Pure]
    [MustDisposeResource]
    protected static TemporaryFile CreateTapFile()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        using var stream = new MemoryStream();
        TapFormat.Instance.Write(tap, stream);
        stream.Position = 0;
        return TemporaryFile.Create(stream, "test.tap");
    }

    [Pure]
    [MustDisposeResource]
    protected static TemporaryFile CreateTzxFile()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        stream.WriteByte(0x10);            // StandardSpeedData block type
        stream.Write([0xE8, 0x03]);        // pause 1000ms
        stream.Write([0x04, 0x00]);        // 4 data bytes
        stream.Write([0xFF, 0x01, 0x02, 0x00]);
        stream.Position = 0;
        return TemporaryFile.Create(stream, "test.tzx");
    }

    [Pure]
    [MustDisposeResource]
    protected static TemporaryFile CreatePzxFile()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        stream.Position = 0;
        return TemporaryFile.Create(stream, "test.pzx");
    }

    [Pure]
    [MustDisposeResource]
    protected static TemporaryFile CreateZ80File()
    {
        var memory = new byte[48 * 1024];
        var snapshot = Z80SnapshotV1File.Create48k(memory);
        snapshot.Header.Registers.PC = 0x1000;
        using var stream = new MemoryStream();
        snapshot.Write(stream);
        stream.Position = 0;
        return TemporaryFile.Create(stream, "test.z80");
    }
}