namespace MrKWatkins.OakIO.Wasm.Tests;

public sealed class GetInfoTests : WasmTestFixture
{
    [Test]
    public void GetInfo_TapFile_ReturnsFormatAndBlocks()
    {
        var result = GetInfo("test.tap", CreateTapData());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[0].Should().Equal("Format: TAP Tape");
        lines[1].Should().Equal("Blocks: 2");
    }

    [Test]
    public void GetInfo_TapFile_ShowsHeaderBlock()
    {
        var result = GetInfo("test.tap", CreateTapData());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[2].Should().Equal("  1: Bytes: test");
    }

    [Test]
    public void GetInfo_TapFile_ShowsDataBlock()
    {
        var result = GetInfo("test.tap", CreateTapData());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[3].Should().Equal("  2: Data: 2 bytes");
    }

    [Test]
    public void GetInfo_TzxFile_ReturnsFormatAndBlocks()
    {
        var result = GetInfo("test.tzx", CreateTzxData());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[0].Should().Equal("Format: TZX Tape");
        lines[1].Should().Equal("Blocks: 1");
    }

    [Test]
    public void GetInfo_PzxFile_ReturnsFormat()
    {
        var result = GetInfo("test.pzx", CreatePzxData());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[0].Should().Equal("Format: PZX Tape");
    }

    [Test]
    public void GetInfo_Z80File_ReturnsFormat()
    {
        var result = GetInfo("test.z80", CreateZ80Data());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[0].Should().Equal("Format: Z80 Snapshot");
    }

    [Test]
    public void GetInfo_Z80File_ReturnsRegisters()
    {
        var result = GetInfo("test.z80", CreateZ80Data());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines[7].Should().Equal("PC: 0x1000");
    }

    [Test]
    public void GetInfo_UnsupportedExtension_Throws()
    {
        AssertThat.Invoking(() => GetInfo("test.blah", CreateTapData()))
            .Should().Throw<NotSupportedException>();
    }
}
