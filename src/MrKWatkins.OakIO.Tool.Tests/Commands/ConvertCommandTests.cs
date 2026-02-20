using MrKWatkins.OakIO.Testing;
using MrKWatkins.OakIO.Tool.Commands;
using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Tests.Commands;

public sealed class ConvertCommandTests : ToolTestFixture
{
    [Test]
    public void Execute_TapToWav_ReturnsSuccess()
    {
        using var inputFile = CreateTapFile();
        using var outputDirectory = TemporaryDirectory.Create();
        var outputPath = outputDirectory.GetFilePath("output.wav");

        RunConvertCommand(inputFile.Path, outputPath).Should().Equal(0);
    }

    [Test]
    public void Execute_TapToWav_CreatesOutputFile()
    {
        using var inputFile = CreateTapFile();
        using var outputDirectory = TemporaryDirectory.Create();
        var outputPath = outputDirectory.GetFilePath("output.wav");

        RunConvertCommand(inputFile.Path, outputPath);

        File.Exists(outputPath).Should().BeTrue();
    }

    [Test]
    public void Execute_TzxToWav_ReturnsSuccess()
    {
        using var inputFile = CreateTzxFile();
        using var outputDirectory = TemporaryDirectory.Create();
        var outputPath = outputDirectory.GetFilePath("output.wav");

        RunConvertCommand(inputFile.Path, outputPath).Should().Equal(0);
    }

    [Test]
    public void Execute_PzxToWav_ReturnsSuccess()
    {
        using var inputFile = CreatePzxFile();
        using var outputDirectory = TemporaryDirectory.Create();
        var outputPath = outputDirectory.GetFilePath("output.wav");

        RunConvertCommand(inputFile.Path, outputPath).Should().Equal(0);
    }

    [Test]
    public void Execute_MissingInputFile_ReturnsError()
    {
        using var outputDirectory = TemporaryDirectory.Create();
        var outputPath = outputDirectory.GetFilePath("output.wav");

        RunConvertCommand("/non/existent/file.tap", outputPath).Should().NotEqual(0);
    }

    [Test]
    public void Execute_UnsupportedConversion_ReturnsError()
    {
        using var inputFile = CreateZ80File();
        using var outputDirectory = TemporaryDirectory.Create();
        var outputPath = outputDirectory.GetFilePath("output.wav");

        RunConvertCommand(inputFile.Path, outputPath).Should().NotEqual(0);
    }

    private static int RunConvertCommand(string inputPath, string outputPath)
    {
        var app = new CommandApp();
        app.Configure(config => config.AddCommand<ConvertCommand>("convert"));
        return app.Run(["convert", inputPath, outputPath]);
    }
}

