using MrKWatkins.OakIO.Compression;
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

        var result = WavFormat.Instance.Read(outputFile.Bytes);
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

        var result = WavFormat.Instance.Read(outputFile.Bytes);
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

        var result = WavFormat.Instance.Read(outputFile.Bytes);
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

    [Test]
    public async Task ExecuteAsync_TapToWav()
    {
        using var inputFile = CreateTapFile();
        using var outputFile = TemporaryFile.Create("output.wav");

        await using (var inputStream = inputFile.OpenRead())
        await using (var outputStream = outputFile.OpenWrite())
        {
            await ConvertCommand.ExecuteAsync(inputFile.Path, inputStream, outputFile.Path, outputStream);
        }

        var result = WavFormat.Instance.Read(outputFile.Bytes);
        result.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public async Task ExecuteAsync_UnsupportedInputExtension_Throws()
    {
        using var inputFile = CreateTapFile();
        using var outputFile = TemporaryFile.Create("output.wav");

        await using var inputStream = inputFile.OpenRead();
        await using var outputStream = outputFile.OpenWrite();

        await "input.blah".Awaiting(f => ConvertCommand.ExecuteAsync(f, inputStream, outputFile.Path, outputStream))
            .Should().ThrowAsync<NotSupportedException>();
    }

    [Test]
    public async Task ExecuteAsync_UnsupportedOutputExtension_Throws()
    {
        using var inputFile = CreateTapFile();
        using var outputFile = TemporaryFile.Create("output.blah");

        await using var inputStream = inputFile.OpenRead();
        await using var outputStream = outputFile.OpenWrite();

        await outputFile.Path.Awaiting(f => ConvertCommand.ExecuteAsync(inputFile.Path, inputStream, f, outputStream))
            .Should().ThrowAsync<NotSupportedException>();
    }

    [Test]
    public async Task ExecuteAsync_UnsupportedConversion_Throws()
    {
        using var inputFile = CreateZ80File();
        using var outputFile = TemporaryFile.Create("output.wav");

        await using var inputStream = inputFile.OpenRead();
        await using var outputStream = outputFile.OpenWrite();

        await inputFile.Path.Awaiting(f => ConvertCommand.ExecuteAsync(f, inputStream, outputFile.Path, outputStream))
            .Should().ThrowAsync<NotSupportedException>();
    }

    [Test]
    public async Task ExecuteAsync_ByteArray_TapToWav()
    {
        using var inputFile = CreateTapFile();
        var result = await ConvertCommand.ExecuteAsync(inputFile.Path, inputFile.Bytes, "output.wav");
        result.Should().NotBeEmpty();

        var wav = WavFormat.Instance.Read(result);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public async Task ExecuteAsync_ByteArray_TapToWav_CompressedZip()
    {
        using var inputFile = CreateTapFile();
        var result = await ConvertCommand.ExecuteAsync(inputFile.Path, inputFile.Bytes, "output.wav", CompressionFormat.Zip);

        using var stream = new MemoryStream(result);
        var wav = (WavFile)await IOFileFormat.LoadAsync("output.zip", stream, [WavFormat.Instance]);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_ByteArray_TapToWav()
    {
        using var inputFile = CreateTapFile();
        var result = ConvertCommand.Execute(inputFile.Path, inputFile.Bytes, "output.wav");
        result.Should().NotBeEmpty();

        var wav = WavFormat.Instance.Read(result);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_ByteArray_TapToWav_CompressedZip()
    {
        using var inputFile = CreateTapFile();
        var result = ConvertCommand.Execute(inputFile.Path, inputFile.Bytes, "output.wav", CompressionFormat.Zip);

        using var stream = new MemoryStream(result);
        var wav = (WavFile)IOFileFormat.Load("output.zip", stream, WavFormat.Instance);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_ByteArray_TapToWav_CompressedGZip()
    {
        using var inputFile = CreateTapFile();
        var result = ConvertCommand.Execute(inputFile.Path, inputFile.Bytes, "output.wav", CompressionFormat.GZip);

        using var stream = new MemoryStream(result);
        var wav = (WavFile)IOFileFormat.Load("output.wav.gz", stream, WavFormat.Instance);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_Stream_TapToWav_CompressedGZip()
    {
        using var inputFile = CreateTapFile();
        using var outputFile = TemporaryFile.Create("output.wav");

        using (var inputStream = inputFile.OpenRead())
        using (var outputStream = outputFile.OpenWrite())
        {
            ConvertCommand.Execute(inputFile.Path, inputStream, outputFile.Path, outputStream, CompressionFormat.GZip);
        }

        using var readStream = new MemoryStream(outputFile.Bytes);
        var wav = (WavFile)IOFileFormat.Load("output.wav.gz", readStream, WavFormat.Instance);
        wav.SampleData.Should().NotBeEmpty();
    }
}