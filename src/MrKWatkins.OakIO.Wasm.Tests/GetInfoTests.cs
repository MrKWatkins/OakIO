namespace MrKWatkins.OakIO.Wasm.Tests;

public sealed class GetInfoTests : WasmTestFixture
{
    [Test]
    public void GetInfo_TapFile_ReturnsJson()
    {
        var result = GetInfo("test.tap", CreateTapData());
        result.Should().StartWith("{");
        result.Should().Contain("\"format\":\"TAP Tape\"");
    }

    [Test]
    public void GetInfo_TapFile_ReturnsFormatAndBlocks()
    {
        var result = GetFileInfo("test.tap", CreateTapData());
        result.Format.Should().Equal("TAP Tape");
        result.Sections[0].Title.Should().Equal("Blocks");
        result.Sections[0].Items!.Count.Should().Equal(2);
    }

    [Test]
    public void GetInfo_TapFile_ShowsHeaderBlock()
    {
        var result = GetFileInfo("test.tap", CreateTapData());
        result.Sections[0].Items![0].Title.Should().Equal("Bytes: test");
    }

    [Test]
    public void GetInfo_TapFile_ShowsDataBlock()
    {
        var result = GetFileInfo("test.tap", CreateTapData());
        result.Sections[0].Items![1].Title.Should().Equal("Data: 2 bytes");
    }

    [Test]
    public void GetInfo_TzxFile_ReturnsFormatAndBlocks()
    {
        var result = GetFileInfo("test.tzx", CreateTzxData());
        result.Format.Should().Equal("TZX Tape");
        result.Sections[0].Title.Should().Equal("Blocks");
        result.Sections[0].Items!.Count.Should().Equal(1);
    }

    [Test]
    public void GetInfo_PzxFile_ReturnsFormat()
    {
        var result = GetFileInfo("test.pzx", CreatePzxData());
        result.Format.Should().Equal("PZX Tape");
    }

    [Test]
    public void GetInfo_Z80File_ReturnsFormat()
    {
        var result = GetFileInfo("test.z80", CreateZ80Data());
        result.Format.Should().Equal("Z80 Snapshot");
    }

    [Test]
    public void GetInfo_Z80File_ReturnsRegisters()
    {
        var result = GetFileInfo("test.z80", CreateZ80Data());
        var registers = result.Sections.Single(s => s.Title == "Registers");
        registers.Properties!.Single(p => p.Name == "PC").Value.Should().Equal("0x1000");
    }

    [Test]
    public void GetInfo_UnsupportedExtension_Throws()
    {
        AssertThat.Invoking(() => GetInfo("test.blah", CreateTapData()))
            .Should().Throw<NotSupportedException>();
    }
}