using System.Text.Json;
using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.Compression;
using MrKWatkins.OakIO.Wav;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.Wasm.Tests;

// OakIOInterop is annotated [SupportedOSPlatform("browser")], but its methods are plain marshalling
// shims over MrKWatkins.OakIO.Commands with no browser-specific logic of their own, so calling them
// directly here on desktop .NET genuinely exercises the same code that runs under WASM.
#pragma warning disable CA1416
public sealed class OakIOInteropTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Test]
    public async Task GetInfo_TapFile_ReturnsJson()
    {
        var result = await GetInfo("test.tap", CreateTapData());
        result.Should().StartWith("{");
        result.Should().Contain("\"format\":\"TAP Tape\"");
    }

    [Test]
    public async Task GetInfo_TapFile_ReturnsFormatAndBlocks()
    {
        var result = await GetFileInfo("test.tap", CreateTapData());
        result.Format.Should().Equal("TAP Tape");
        result.Sections[0].Title.Should().Equal("Blocks");
        result.Sections[0].Items.Count.Should().Equal(2);
    }

    [Test]
    public async Task GetInfo_TapFile_ShowsHeaderBlock()
    {
        var result = await GetFileInfo("test.tap", CreateTapData());
        result.Sections[0].Items[0].Title.Should().Equal("Bytes: test");
    }

    [Test]
    public async Task GetInfo_TapFile_ShowsDataBlock()
    {
        var result = await GetFileInfo("test.tap", CreateTapData());
        result.Sections[0].Items[1].Title.Should().Equal("Data: 2 bytes");
    }

    [Test]
    public async Task GetInfo_TzxFile_ReturnsFormatAndBlocks()
    {
        var result = await GetFileInfo("test.tzx", CreateTzxData());
        result.Format.Should().Equal("TZX Tape");
        result.Sections[0].Title.Should().Equal("Blocks");
        result.Sections[0].Items.Count.Should().Equal(1);
    }

    [Test]
    public async Task GetInfo_PzxFile_ReturnsFormat()
    {
        var result = await GetFileInfo("test.pzx", CreatePzxData());
        result.Format.Should().Equal("PZX Tape");
    }

    [Test]
    public async Task GetInfo_Z80File_ReturnsFormat()
    {
        var result = await GetFileInfo("test.z80", CreateZ80Data());
        result.Format.Should().Equal("Z80 Snapshot");
    }

    [Test]
    public async Task GetInfo_Z80File_ReturnsRegisters()
    {
        var result = await GetFileInfo("test.z80", CreateZ80Data());
        var registers = result.Sections.Single(s => s.Title == "Registers");
        registers.Properties.Single(p => p.Name == "PC").Value.Should().Equal("0x1000");
    }

    [Test]
    public async Task GetInfo_UnsupportedExtension_Throws()
    {
        await "test.blah".Awaiting(f => GetInfo(f, CreateTapData()))
            .Should().ThrowAsync<NotSupportedException>();
    }

    [Test]
    public async Task Convert_TapToWav_ReturnsNonEmptyData()
    {
        var result = await Convert("test.tap", CreateTapData(), "output.wav");
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task Convert_TapToWav_ProducesValidWav()
    {
        var result = await Convert("test.tap", CreateTapData(), "output.wav");
        var wav = WavFormat.Instance.Read(result);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public async Task Convert_TzxToWav_ReturnsNonEmptyData()
    {
        var result = await Convert("test.tzx", CreateTzxData(), "output.wav");
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task Convert_TzxToWav_ProducesValidWav()
    {
        var result = await Convert("test.tzx", CreateTzxData(), "output.wav");
        var wav = WavFormat.Instance.Read(result);
        wav.SampleRate.Should().Equal(44100u);
    }

    [Test]
    public async Task Convert_PzxToWav_ReturnsNonEmptyData()
    {
        var result = await Convert("test.pzx", CreatePzxData(), "output.wav");
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task Convert_PzxToWav_ProducesValidWav()
    {
        var result = await Convert("test.pzx", CreatePzxData(), "output.wav");
        var wav = WavFormat.Instance.Read(result);
        wav.SampleRate.Should().Equal(44100u);
    }

    [Test]
    public async Task Convert_TapToWav_CompressedZip_ProducesValidZip()
    {
        var result = await Convert("test.tap", CreateTapData(), "output.wav", CompressionFormat.Zip);

        using var stream = new MemoryStream(result);
        var wav = (WavFile)await IOFileFormat.LoadAsync("output.zip", stream, [WavFormat.Instance]);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public async Task Convert_UnsupportedInputExtension_Throws()
    {
        await "test.blah".Awaiting(f => Convert(f, CreateTapData(), "output.wav"))
            .Should().ThrowAsync<NotSupportedException>();
    }

    [Test]
    public async Task Convert_UnsupportedOutputExtension_Throws()
    {
        await "output.blah".Awaiting(f => Convert("test.tap", CreateTapData(), f))
            .Should().ThrowAsync<NotSupportedException>();
    }

    [Test]
    public async Task Convert_UnsupportedConversion_Throws()
    {
        await "test.z80".Awaiting(f => Convert(f, CreateZ80Data(), "output.wav"))
            .Should().ThrowAsync<NotSupportedException>();
    }

    [TestCase(CompressionFormat.None, "output.wav")]
    [TestCase(CompressionFormat.Zip, "output.zip")]
    [TestCase(CompressionFormat.GZip, "output.wav.gz")]
    public void GetCompressedFilename_ReturnsExpectedFilename(CompressionFormat compressionFormat, string expected)
    {
        GetCompressedFilename("output.wav", compressionFormat).Should().Equal(expected);
    }

    [Pure]
    private static Task<string> GetInfo(string inputFilename, byte[] inputData) =>
        OakIOInterop.GetInfo(inputFilename, inputData);

    [Pure]
    private static async Task<FileInfoResult> GetFileInfo(string inputFilename, byte[] inputData)
    {
        var json = await GetInfo(inputFilename, inputData).ConfigureAwait(false);
        return JsonSerializer.Deserialize<FileInfoResult>(json, JsonOptions)!;
    }

    [Pure]
    private static async Task<byte[]> Convert(string inputFilename, byte[] inputData, string outputFilename, CompressionFormat compressionFormat = CompressionFormat.None)
    {
        var base64 = await OakIOInterop.Convert(inputFilename, inputData, outputFilename, compressionFormat.ToString()).ConfigureAwait(false);
        return System.Convert.FromBase64String(base64);
    }

    [Pure]
    private static string GetCompressedFilename(string filename, CompressionFormat compressionFormat) =>
        OakIOInterop.GetCompressedFilename(filename, compressionFormat.ToString());

    [Pure]
    private static byte[] CreateTapData()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        using var stream = new MemoryStream();
        tap.Write(stream);
        return stream.ToArray();
    }

    [Pure]
    private static byte[] CreateTzxData()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        stream.WriteByte(0x10);
        stream.Write([0xE8, 0x03]);
        stream.Write([0x04, 0x00]);
        stream.Write([0xFF, 0x01, 0x02, 0x00]);
        return stream.ToArray();
    }

    [Pure]
    private static byte[] CreatePzxData()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        return stream.ToArray();
    }

    [Pure]
    private static byte[] CreateZ80Data()
    {
        var memory = new byte[48 * 1024];
        var snapshot = Z80V1File.Create48k(memory);
        snapshot.Header.Registers.PC = 0x1000;
        using var stream = new MemoryStream();
        snapshot.Write(stream);
        return stream.ToArray();
    }
}