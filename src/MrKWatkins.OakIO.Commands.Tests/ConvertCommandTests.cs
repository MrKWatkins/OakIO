using MrKWatkins.OakIO.Testing;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Commands.Tests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class ConvertCommandTests : CommandsTestFixture
{
    [Test]
    public void Execute_TapToWav()
    {
        using var inputFile = CreateTapFile();
        using var outputFile = TemporaryFile.Create("output.wav");

        using (var inputStream = inputFile.OpenRead())
        using (var outputStream = outputFile.OpenWrite())
        {
            ConvertCommand.Execute(inputFile.Path, inputStream, outputFile.Path, outputStream);
        }

        var result = (WavFile)WavFormat.Instance.Read(outputFile.Bytes);
        result.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_TzxToWav()
    {
        using var inputFile = CreateTzxFile();
        using var outputFile = TemporaryFile.Create("output.wav");

        using (var inputStream = inputFile.OpenRead())
        using (var outputStream = outputFile.OpenWrite())
        {
            ConvertCommand.Execute(inputFile.Path, inputStream, outputFile.Path, outputStream);
        }

        var result = (WavFile)WavFormat.Instance.Read(outputFile.Bytes);
        result.SampleRate.Should().Equal(44100u);
    }

    [Test]
    public void Execute_PzxToWav()
    {
        using var inputFile = CreatePzxFile();
        using var outputFile = TemporaryFile.Create("output.wav");

        using (var inputStream = inputFile.OpenRead())
        using (var outputStream = outputFile.OpenWrite())
        {
            ConvertCommand.Execute(inputFile.Path, inputStream, outputFile.Path, outputStream);
        }

        var result = (WavFile)WavFormat.Instance.Read(outputFile.Bytes);
        result.SampleRate.Should().Equal(44100u);
    }

    [Test]
    public void Execute_UnsupportedInputExtension_Throws()
    {
        using var inputFile = CreateTapFile();
        using var outputFile = TemporaryFile.Create("output.wav");

        using var inputStream = inputFile.OpenRead();
        using var outputStream = outputFile.OpenWrite();

        AssertThat.Invoking(() => ConvertCommand.Execute("input.blah", inputStream, outputFile.Path, outputStream))
            .Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Execute_UnsupportedOutputExtension_Throws()
    {
        using var inputFile = CreateTapFile();
        using var outputFile = TemporaryFile.Create("output.blah");

        using var inputStream = inputFile.OpenRead();
        using var outputStream = outputFile.OpenWrite();

        AssertThat.Invoking(() => ConvertCommand.Execute(inputFile.Path, inputStream, outputFile.Path, outputStream))
            .Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Execute_UnsupportedConversion_Throws()
    {
        using var inputFile = CreateZ80File();
        using var outputFile = TemporaryFile.Create("output.wav");

        using var inputStream = inputFile.OpenRead();
        using var outputStream = outputFile.OpenWrite();

        AssertThat.Invoking(() => ConvertCommand.Execute(inputFile.Path, inputStream, outputFile.Path, outputStream))
            .Should().Throw<NotSupportedException>();
    }
}