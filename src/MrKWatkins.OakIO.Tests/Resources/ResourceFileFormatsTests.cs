using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;
using MrKWatkins.OakIO.Compression;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ResourceFileFormatsTests
{
    [Test]
    public void AllFormats()
    {
        ResourceFileFormats.AllFormats.Should().SequenceEqual(BmpFormat.Instance, GplFormat.Instance, JascFormat.Instance, PaintNetFormat.Instance);
        using var stream = new MemoryStream(Bmp.BmpFormatTests.Create(24, false));
        var file = IOFileFormat.Load("image.bmp", stream, ResourceFileFormats.AllFormats);
        file.Should().BeOfType<BmpFile>().Value.Image.GetPixel(2, 1).Should().Equal(new Colour(5, 10, 20));
    }

    [TestCase("palette.gpl", typeof(GplFile), "GIMP Palette\n1 2 3")]
    [TestCase("palette.pal", typeof(JascFile), "JASC-PAL\n0100\n1\n1 2 3")]
    [TestCase("palette.txt", typeof(PaintNetFile), "FF010203")]
    public void AllFormats_Palettes(string filename, Type expectedType, string text)
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
        var file = IOFileFormat.Load(filename, stream, ResourceFileFormats.AllFormats);
        file.GetType().Should().Equal(expectedType);
        ((PaletteFile)file).Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3));
    }

    [Test]
    public void AllFormats_CompressedPalette()
    {
        using var stream = new MemoryStream();
        new JascFile(new PaletteData([new Colour(1, 2, 3)])).Write(stream, "palette.pal", CompressionFormat.GZip);
        stream.Position = 0;
        IOFileFormat.Load("palette.pal.gz", stream, ResourceFileFormats.AllFormats).Should().BeOfType<JascFile>()
            .Value.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3));
    }
}