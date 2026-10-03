using MrKWatkins.OakIO.Compression;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources;

public sealed class ZXSpectrumNextResourceFileFormatsTests
{
    [Test]
    public void AllFormats()
    {
        ZXSpectrumNextResourceFileFormats.AllFormats.Should().SequenceEqual([.. ResourceFileFormats.AllFormats, NxpFormat.Instance]);
        ResourceFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("nxp");
        ZXSpectrumNextFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("nxp");
    }

    [TestCase(16)]
    [TestCase(256)]
    public void AllFormats_LoadNxp(int count)
    {
        var bytes = Nxp.NxpFormatTests.CreateBytes(count);
        using var stream = new MemoryStream(bytes);
        var result = IOFileFormat.Load("palette.nxp", stream, ZXSpectrumNextResourceFileFormats.AllFormats);
        result.Should().BeOfType<NxpFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public async Task AllFormats_LoadNxpAsync()
    {
        var bytes = Nxp.NxpFormatTests.CreateBytes(16);
        using var stream = new MemoryStream(bytes);
        var result = await IOFileFormat.LoadAsync("palette.nxp", stream, ZXSpectrumNextResourceFileFormats.AllFormats);
        result.Should().BeOfType<NxpFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void AllFormats_LoadCompressedNxp()
    {
        var bytes = Nxp.NxpFormatTests.CreateBytes(16);
        var file = NxpFormat.Instance.Read(bytes);
        using var stream = new MemoryStream();
        file.Write(stream, "palette.nxp", CompressionFormat.GZip);
        stream.Position = 0;
        IOFileFormat.Load("palette.nxp.gz", stream, ZXSpectrumNextResourceFileFormats.AllFormats)
            .Should().BeOfType<NxpFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void AllFormats_LoadGeneral()
    {
        var bytes = new JascFile(new PaletteData([new Colour(1, 2, 3)])).ToByteArray();
        using var stream = new MemoryStream(bytes);
        IOFileFormat.Load("palette.pal", stream, ZXSpectrumNextResourceFileFormats.AllFormats)
            .Should().BeOfType<JascFile>().Value.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3));
    }
}