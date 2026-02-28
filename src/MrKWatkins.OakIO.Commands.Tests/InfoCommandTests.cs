using MrKWatkins.OakIO.Testing;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.Commands.Tests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class InfoCommandTests : CommandsTestFixture
{
    [Test]
    public void Execute_TapFile()
    {
        using var inputFile = CreateTapFile();
        var lines = RunInfoCommand(inputFile);
        lines[0].Should().Equal("Format: TAP Tape");
        lines[1].Should().Equal("Blocks: 2");
        lines[2].Should().StartWith("    1:");
        lines[3].Should().StartWith("    2:");
    }

    [Test]
    public void Execute_TapFile_ShowsHeaderBlock()
    {
        using var inputFile = CreateTapFile();
        var lines = RunInfoCommand(inputFile);
        lines[2].Should().StartWith("    1: Bytes: test");
    }

    [Test]
    public void Execute_TapFile_ShowsDataBlock()
    {
        using var inputFile = CreateTapFile();
        var lines = RunInfoCommand(inputFile);
        lines[3].Should().StartWith("    2: Data: 2 bytes");
    }

    [Test]
    public void Execute_TzxFile()
    {
        using var inputFile = CreateTzxFile();
        var lines = RunInfoCommand(inputFile);
        lines[0].Should().Equal("Format: TZX Tape");
        lines[1].Should().Equal("Blocks: 1");
        lines[2].Should().StartWith("    1:");
    }

    [Test]
    public void Execute_TzxFile_ShowsBlocks()
    {
        using var inputFile = CreateTzxFile();
        var lines = RunInfoCommand(inputFile);
        lines[2].Should().StartWith("    1: Standard Speed Data");
    }

    [Test]
    public void Execute_PzxFile()
    {
        using var inputFile = CreatePzxFile();
        var lines = RunInfoCommand(inputFile);
        lines[0].Should().Equal("Format: PZX Tape");
        lines.Should().HaveCount(1);
    }

    [Test]
    public void Execute_PzxFile_ShowsBlocks()
    {
        using var inputFile = CreateTzxFile();
        var lines = RunInfoCommand(inputFile);
        lines[1].Should().Equal("Blocks: 1");
    }

    [Test]
    public void Execute_Z80File_ShowsFormat()
    {
        using var inputFile = CreateZ80File();
        var lines = RunInfoCommand(inputFile);
        lines[0].Should().Equal("Format: Z80 Snapshot");
    }

    [Test]
    public void Execute_Z80File_ShowsRegisters()
    {
        using var inputFile = CreateZ80File();
        var lines = RunInfoCommand(inputFile);
        var snapshot = (Z80V1File)Z80Format.Instance.Read(inputFile.Bytes);
        var registers = snapshot.Registers;

        var registersIndex = FindLine(lines, "Registers:");
        registersIndex.Should().NotEqual(-1);
        lines[registersIndex + 1].Should().Equal($"    AF: 0x{registers.AF:X4}");
        lines[registersIndex + 2].Should().Equal($"    BC: 0x{registers.BC:X4}");
        lines[registersIndex + 3].Should().Equal($"    DE: 0x{registers.DE:X4}");
        lines[registersIndex + 4].Should().Equal($"    HL: 0x{registers.HL:X4}");
        lines[registersIndex + 5].Should().Equal($"    IX: 0x{registers.IX:X4}");
        lines[registersIndex + 6].Should().Equal($"    IY: 0x{registers.IY:X4}");
        lines[registersIndex + 7].Should().Equal($"    PC: 0x{registers.PC:X4}");
        lines[registersIndex + 8].Should().Equal($"    SP: 0x{registers.SP:X4}");
        lines[registersIndex + 9].Should().Equal($"    IR: 0x{registers.IR:X4}");
    }

    [Test]
    public void Execute_UnsupportedExtension_Throws()
    {
        using var inputFile = CreateTapFile();
        using var inputStream = inputFile.OpenRead();
        using var output = new StringWriter();
        AssertThat.Invoking(() => InfoCommand.Execute("test.blah", inputStream, output))
            .Should().Throw<NotSupportedException>();
    }

    [Test]
    public void GetFileInfo_TapFile()
    {
        using var inputFile = CreateTapFile();
        var result = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);
        result.Format.Should().Equal("TAP Tape");
        result.FileExtension.Should().Equal("tap");
        result.Type.Should().Equal("tape");
        result.ConvertibleTo.Should().HaveCount(3);
        result.Sections.Count.Should().Equal(1);
        result.Sections[0].Title.Should().Equal("Blocks");
        result.Sections[0].Items.Count.Should().Equal(2);
        result.Sections[0].Items[0].Title.Should().Equal("Bytes: test");
        result.Sections[0].Items[1].Title.Should().Equal("Data: 2 bytes");
    }

    [Test]
    public void GetFileInfo_TzxFile()
    {
        using var inputFile = CreateTzxFile();
        var result = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);
        result.Format.Should().Equal("TZX Tape");
        result.FileExtension.Should().Equal("tzx");
        result.Type.Should().Equal("tape");
        result.Sections.Should().HaveCount(1);
        result.Sections[0].Title.Should().Equal("Blocks");
        result.Sections[0].Items.Count.Should().Equal(1);
        result.Sections[0].Items[0].Title.Should().Equal("Standard Speed Data");
    }

    [Test]
    public void GetFileInfo_PzxFile()
    {
        using var inputFile = CreatePzxFile();
        var result = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);
        result.Format.Should().Equal("PZX Tape");
        result.FileExtension.Should().Equal("pzx");
        result.Type.Should().Equal("tape");
        result.Sections.Should().HaveCount(1);
        result.Sections[0].Title.Should().Equal("Blocks");
        result.Sections[0].Items.Count.Should().Equal(0);
    }

    [Test]
    public void GetFileInfo_Z80File()
    {
        using var inputFile = CreateZ80File();
        var result = InfoCommand.GetFileInfo(inputFile.Path, inputFile.Bytes);
        result.Format.Should().Equal("Z80 Snapshot");
        result.FileExtension.Should().Equal("z80");
        result.Type.Should().Equal("snapshot");

        var registersSection = result.Sections.Single(s => s.Title == "Registers");
        registersSection.Properties.Count.Should().Equal(9);
        registersSection.Properties[0].Name.Should().Equal("AF");
        registersSection.Properties[0].Format.Should().Equal("hex");

        var shadowSection = result.Sections.Single(s => s.Title == "Shadow Registers");
        shadowSection.Properties.Count.Should().Equal(4);
    }

    [Test]
    public void GetFileInfoJson_TapFile()
    {
        using var inputFile = CreateTapFile();
        var json = InfoCommand.GetFileInfoJson(inputFile.Path, inputFile.Bytes);
        json.Should().StartWith("{");
        json.Should().Contain("\"format\":\"TAP Tape\"");
    }

    [Pure]
    private static IReadOnlyList<string> RunInfoCommand(TemporaryFile inputFile)
    {
        using var inputStream = inputFile.OpenRead();
        using var output = new StringWriter();
        InfoCommand.Execute(inputFile.Path, inputStream, output);
        return output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    }

    [Pure]
    private static int FindLine(IReadOnlyList<string> lines, string text)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] == text)
            {
                return i;
            }
        }

        return -1;
    }
}