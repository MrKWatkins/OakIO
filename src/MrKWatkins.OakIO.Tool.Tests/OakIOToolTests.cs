using Spectre.Console.Cli;

namespace MrKWatkins.OakIO.Tool.Tests;

public sealed class OakIOToolTests : ToolTestFixture
{
    [Test]
    public void Configure_RegistersInfoCommand()
    {
        using var inputFile = CreateTapFile();

        var app = new CommandApp();
        app.Configure(OakIOTool.Configure);

        app.Run(["info", inputFile.Path]).Should().Equal(0);
    }

    [Test]
    public void Configure_RegistersConvertCommand()
    {
        using var inputFile = CreateTapFile();
        using var outputDirectory = Testing.TemporaryDirectory.Create();
        var outputPath = outputDirectory.GetFilePath("output.wav");

        var app = new CommandApp();
        app.Configure(OakIOTool.Configure);

        app.Run(["convert", inputFile.Path, outputPath]).Should().Equal(0);
    }
}