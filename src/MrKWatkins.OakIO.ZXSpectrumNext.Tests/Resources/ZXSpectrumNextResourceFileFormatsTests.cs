using MrKWatkins.OakIO.Compression;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.ZXSpectrum.Resources;
using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources;

public sealed class ZXSpectrumNextResourceFileFormatsTests
{
    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public async Task AllFormats_LoadNxiAsync(int width, int height)
    {
        var bytes = Nxi.NxiTestData.Bytes(width, height);
        using var stream = new MemoryStream(bytes);
        var file = (NxiFile)await IOFileFormat.LoadAsync("screen.nxi", stream, ZXSpectrumNextResourceFileFormats.AllFormats);
        file.PixelData.Width.Should().Equal(width);
        file.PixelData.Height.Should().Equal(height);
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase(256, 192, false)]
    [TestCase(320, 256, false)]
    [TestCase(640, 256, false)]
    [TestCase(256, 192, true)]
    [TestCase(320, 256, true)]
    [TestCase(640, 256, true)]
    public void AllFormats_LoadNxi(int width, int height, bool compressed)
    {
        var bytes = Nxi.NxiTestData.Bytes(width, height);
        using var stream = new MemoryStream();
        var original = NxiFormat.Instance.Read(bytes);
        if (compressed)
        {
            original.Write(stream, "screen.nxi", CompressionFormat.GZip);
        }
        else
        {
            original.Write(stream);
        }
        stream.Position = 0;
        var file = (NxiFile)IOFileFormat.Load(compressed ? "screen.nxi.gz" : "screen.nxi", stream, ZXSpectrumNextResourceFileFormats.AllFormats);
        file.PixelData.Width.Should().Equal(width);
        file.PixelData.Height.Should().Equal(height);
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void AllFormats_LoadScr()
    {
        var bytes = Enumerable.Range(0, 6912).Select(index => (byte)(index * 37)).ToArray();
        using var stream = new MemoryStream(bytes);
        IOFileFormat.Load("screen.scr", stream, ZXSpectrumNextResourceFileFormats.AllFormats)
            .Should().BeOfType<ScrFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }
    [TestCase(4, 1)]
    [TestCase(4, 4)]
    [TestCase(4, 8)]
    [TestCase(8, 1)]
    [TestCase(8, 4)]
    [TestCase(8, 8)]
    public void WithTiles(int spriteBits, int tileBits)
    {
        ZXSpectrumNextResourceFileFormats.WithTiles(spriteBits, tileBits).Should().SequenceEqual(
            [.. ZXSpectrumNextResourceFileFormats.AllFormats, SprFormat.ForBitsPerPixel(spriteBits), NxtFormat.ForBitsPerPixel(tileBits)]);
        ZXSpectrumNextResourceFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("spr");
        ZXSpectrumNextResourceFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("nxt");
    }

    [TestCase(1, 4)]
    [TestCase(4, 2)]
    public void WithTiles_Invalid(int spriteBits, int tileBits) =>
        AssertThat.Invoking(() => ZXSpectrumNextResourceFileFormats.WithTiles(spriteBits, tileBits)).Should().Throw<ArgumentOutOfRangeException>();

    [TestCase("patterns.spr", 16, 4)]
    [TestCase("patterns.spr", 16, 8)]
    [TestCase("tiles.nxt", 8, 1)]
    [TestCase("tiles.nxt", 8, 4)]
    [TestCase("tiles.nxt", 8, 8)]
    public async Task WithTiles_LoadAsync(string filename, int size, int bits)
    {
        var bytes = TileTestData.Bytes(size, bits, 2);
        using var stream = new MemoryStream(bytes);
        var formats = ZXSpectrumNextResourceFileFormats.WithTiles(size == 16 ? bits : 4, size == 8 ? bits : 4);
        var file = (TilesFile)await IOFileFormat.LoadAsync(filename, stream, formats);
        file.GetType().Should().Equal(size == 16 ? typeof(SprFile) : typeof(NxtFile));
        file.Tiles.Count.Should().Equal(2);
        file.Tiles.BitsPerPixel.Should().Equal(bits);
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase("patterns.spr", 16)]
    [TestCase("tiles.nxt", 8)]
    public void WithTiles_LoadCompressed(string filename, int size)
    {
        TilesFile original = size == 16 ? SprFormat.FourBit.Read(TileTestData.Bytes(size, 4, 2)) : NxtFormat.FourBit.Read(TileTestData.Bytes(size, 4, 2));
        using var stream = new MemoryStream();
        original.Write(stream, filename, CompressionFormat.GZip);
        stream.Position = 0;
        var file = (TilesFile)IOFileFormat.Load(filename + ".gz", stream, ZXSpectrumNextResourceFileFormats.WithTiles(4, 4));
        file.Format.Should().BeTheSameInstanceAs(original.Format);
        file.Tiles.Pixels.Should().SequenceEqual(original.Tiles.Pixels);
        file.ToByteArray().Should().SequenceEqual(original.ToByteArray());
    }
    [Test]
    public void AllFormats()
    {
        ZXSpectrumNextResourceFileFormats.AllFormats.Should().SequenceEqual([.. ZXSpectrumResourceFileFormats.AllFormats, NxpFormat.Instance, NxiFormat.Instance]);
        ResourceFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("nxp");
        ResourceFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("nxi");
        ZXSpectrumResourceFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("nxi");
        ZXSpectrumNextFileFormats.AllFormats.Select(format => format.FileExtension).Should().NotContain("nxi");
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