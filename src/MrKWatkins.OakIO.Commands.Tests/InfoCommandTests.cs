using MrKWatkins.OakIO.Testing;
using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;
using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

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
        lines[2].Should().StartWith("  1:");
        lines[3].Should().StartWith("  2:");
    }

    [Test]
    public void Execute_TapFile_ShowsHeaderBlock()
    {
        using var inputFile = CreateTapFile();
        var lines = RunInfoCommand(inputFile);
        lines[2].Should().Equal("  1: Bytes: test");
    }

    [Test]
    public void Execute_TapFile_ShowsDataBlock()
    {
        using var inputFile = CreateTapFile();
        var lines = RunInfoCommand(inputFile);
        lines[3].Should().Equal("  2: Data: 2 bytes");
    }

    [Test]
    public void Execute_TzxFile()
    {
        using var inputFile = CreateTzxFile();
        var lines = RunInfoCommand(inputFile);
        lines[0].Should().Equal("Format: TZX Tape");
        lines[1].Should().Equal("Blocks: 1");
        lines[2].Should().StartWith("  1:");
    }

    [Test]
    public void Execute_TzxFile_ShowsBlocks()
    {
        using var inputFile = CreateTzxFile();
        var lines = RunInfoCommand(inputFile);
        var tzxFile = TzxFormat.Instance.Read(inputFile.Bytes);
        lines[2].Should().Equal($"  1: {tzxFile.Blocks[0]}");
    }

    [Test]
    public void Execute_PzxFile()
    {
        using var inputFile = CreatePzxFile();
        var lines = RunInfoCommand(inputFile);
        lines[0].Should().Equal("Format: PZX Tape");
        lines[1].Should().StartWith("Blocks: ");
    }

    [Test]
    public void Execute_PzxFile_ShowsBlocks()
    {
        using var inputFile = CreatePzxFile();
        var lines = RunInfoCommand(inputFile);
        var pzxFile = PzxFormat.Instance.Read(inputFile.Bytes);
        lines[1].Should().Equal($"Blocks: {pzxFile.Blocks.Count}");
        foreach (var (block, index) in pzxFile.Blocks.Select((b, i) => (b, i)))
        {
            lines[2 + index].Should().Equal($"  {index + 1}: {block}");
        }
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
        var snapshot = (Z80SnapshotV1File)Z80SnapshotFormat.Instance.Read(inputFile.Bytes);
        var registers = snapshot.Registers;
        lines[1].Should().Equal($"AF: 0x{registers.AF:X4}");
        lines[2].Should().Equal($"BC: 0x{registers.BC:X4}");
        lines[3].Should().Equal($"DE: 0x{registers.DE:X4}");
        lines[4].Should().Equal($"HL: 0x{registers.HL:X4}");
        lines[5].Should().Equal($"IX: 0x{registers.IX:X4}");
        lines[6].Should().Equal($"IY: 0x{registers.IY:X4}");
        lines[7].Should().Equal($"PC: 0x{registers.PC:X4}");
        lines[8].Should().Equal($"SP: 0x{registers.SP:X4}");
        lines[9].Should().Equal($"IR: 0x{registers.IR:X4}");
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

    [Pure]
    private static IReadOnlyList<string> RunInfoCommand(TemporaryFile inputFile)
    {
        using var inputStream = inputFile.OpenRead();
        using var output = new StringWriter();
        InfoCommand.Execute(inputFile.Path, inputStream, output);
        return output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    }
}