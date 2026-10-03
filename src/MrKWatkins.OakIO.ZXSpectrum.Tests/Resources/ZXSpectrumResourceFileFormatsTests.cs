using MrKWatkins.OakIO.Compression;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrum.Resources;
using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources;

public sealed class ZXSpectrumResourceFileFormatsTests
{
    [Test]
    public void AllFormats()
    {
        ZXSpectrumResourceFileFormats.AllFormats.Should().SequenceEqual([.. ResourceFileFormats.AllFormats, ScrFormat.Instance]);
        ResourceFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("scr");
        ZXSpectrumFileFormat.AllFormats.Select(format => format.FileExtension).Should().NotContain("scr");
    }
    [Test]
    public void AllFormats_Load()
    {
        var bytes = Scr.ScrTestData.Bytes();
        using var stream = new MemoryStream(bytes);
        IOFileFormat.Load("screen.scr", stream, ZXSpectrumResourceFileFormats.AllFormats)
            .Should().BeOfType<ScrFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }
    [Test]
    public async Task AllFormats_LoadAsync()
    {
        var bytes = Scr.ScrTestData.Bytes();
        using var stream = new MemoryStream(bytes);
        var file = await IOFileFormat.LoadAsync("screen.scr", stream, ZXSpectrumResourceFileFormats.AllFormats);
        file.Should().BeOfType<ScrFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }
    [Test]
    public void AllFormats_LoadCompressed()
    {
        var bytes = Scr.ScrTestData.Bytes();
        using var stream = new MemoryStream();
        new ScrFile(bytes).Write(stream, "screen.scr", CompressionFormat.GZip);
        stream.Position = 0;
        IOFileFormat.Load("screen.scr.gz", stream, ZXSpectrumResourceFileFormats.AllFormats)
            .Should().BeOfType<ScrFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }
}