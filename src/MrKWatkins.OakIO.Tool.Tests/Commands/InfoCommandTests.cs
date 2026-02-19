using MrKWatkins.OakIO.Testing;
using MrKWatkins.OakIO.Tool.Commands;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Tests.Commands;

public sealed class InfoCommandTests : ToolTestFixture
{
    [Test]
    public void Execute_TapFile_ReturnsSuccess()
    {
        using var inputFile = CreateTapFile();
        RunInfoCommand(inputFile.Path).Should().Equal(0);
    }

    [Test]
    public void Execute_TapFile_OutputsFormat()
    {
        using var inputFile = CreateTapFile();
        var output = CaptureInfoCommandOutput(inputFile.Path);
        output.Should().Contain("Format: TAP Tape");
    }

    [Test]
    public void Execute_TapFile_OutputsBlocks()
    {
        using var inputFile = CreateTapFile();
        var output = CaptureInfoCommandOutput(inputFile.Path);
        output.Should().Contain("Blocks: 2");
        output.Should().Contain("Bytes: test");
    }

    [Test]
    public void Execute_TzxFile_ReturnsSuccess()
    {
        using var inputFile = CreateTzxFile();
        RunInfoCommand(inputFile.Path).Should().Equal(0);
    }

    [Test]
    public void Execute_TzxFile_OutputsFormat()
    {
        using var inputFile = CreateTzxFile();
        var output = CaptureInfoCommandOutput(inputFile.Path);
        output.Should().Contain("Format: TZX Tape");
    }

    [Test]
    public void Execute_TzxFile_OutputsBlocks()
    {
        using var inputFile = CreateTzxFile();
        var output = CaptureInfoCommandOutput(inputFile.Path);
        output.Should().Contain("Blocks: 1");
    }

    [Test]
    public void Execute_PzxFile_ReturnsSuccess()
    {
        using var inputFile = CreatePzxFile();
        RunInfoCommand(inputFile.Path).Should().Equal(0);
    }

    [Test]
    public void Execute_PzxFile_OutputsFormat()
    {
        using var inputFile = CreatePzxFile();
        var output = CaptureInfoCommandOutput(inputFile.Path);
        output.Should().Contain("Format: PZX Tape");
    }

    [Test]
    public void Execute_Z80File_ReturnsSuccess()
    {
        using var inputFile = CreateZ80File();
        RunInfoCommand(inputFile.Path).Should().Equal(0);
    }

    [Test]
    public void Execute_Z80File_OutputsFormat()
    {
        using var inputFile = CreateZ80File();
        var output = CaptureInfoCommandOutput(inputFile.Path);
        output.Should().Contain("Format: Z80 Snapshot");
    }

    [Test]
    public void Execute_Z80File_OutputsRegisters()
    {
        using var inputFile = CreateZ80File();
        var output = CaptureInfoCommandOutput(inputFile.Path);
        output.Should().Contain("AF: 0x");
        output.Should().Contain("PC: 0x");
        output.Should().Contain("SP: 0x");
    }

    [Test]
    public void Execute_MissingFile_ReturnsError()
    {
        RunInfoCommand("/non/existent/file.tap").Should().NotEqual(0);
    }

    private static int RunInfoCommand(string inputPath)
    {
        var app = new CommandApp();
        app.Configure(config => config.AddCommand<InfoCommand>("info"));
        return app.Run(["info", inputPath]);
    }

    private static string CaptureInfoCommandOutput(string inputPath)
    {
        using var output = new StringWriter();
        var previous = Console.Out;
        Console.SetOut(output);
        try
        {
            RunInfoCommand(inputPath);
        }
        finally
        {
            Console.SetOut(previous);
        }
        return output.ToString();
    }
}

