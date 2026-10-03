using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources.Scr;

public sealed class ImageToScrConverterTests
{
    [Test]
    public void Constructor()
    {
        var converter = new ImageToScrConverter(BmpFormat.Instance);
        converter.SourceFormat.Should().BeTheSameInstanceAs(BmpFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(ScrFormat.Instance);
    }
    [Test]
    public void Constructor_Null() =>
        AssertThat.Invoking(() => new ImageToScrConverter(null!)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Convert_ImageFile()
    {
        var bytes = ScrTestData.Bytes();
        // Indexed BMP with a reversed palette ensures conversion follows colours, not numeric source indices.
        var expected = ScrTestData.Pixels(bytes, false);
        var source = new BmpFile(new IndexedImageData(256, 192, [.. expected.Select(index => (byte)(15 - index))],
            new PaletteData(ScrTestData.Colours.Reverse()), 4));
        var file = new ImageToScrConverter(source.Format).Convert(source);
        var image = file.Render();
        image.Pixels.Select(index => ScrTestData.Colours[index]).Should().SequenceEqual(expected.Select(index => ScrTestData.Colours[index]));
        file.Attributes.Data.All(attribute => attribute < 128).Should().BeTrue();
    }
    [Test]
    public void Convert_ImageFile_ColourImage()
    {
        var pixels = Enumerable.Repeat(new Colour(0, 192, 192), 256 * 192).ToArray();
        var source = new BmpFile(new ColourImageData(256, 192, pixels));
        var file = new ImageToScrConverter(source.Format).Convert(source);
        file.Bitmap.Data.Should().SequenceEqual(new byte[6144]);
        file.Attributes.Data.Should().SequenceEqual(Enumerable.Repeat((byte)45, 768));
        file.Render().Pixels.Select(index => ScrTestData.Colours[index]).Should().SequenceEqual(pixels);
    }
    [Test]
    public void Convert_ImageFile_Scr()
    {
        var bytes = ScrTestData.Bytes();
        var source = new ScrFile(bytes);
        var copy = new ImageToScrConverter(source.Format).Convert(source);
        copy.Should().NotBeTheSameInstanceAs(source);
        copy.Bitmap.Should().NotBeTheSameInstanceAs(source.Bitmap);
        copy.Attributes.Should().NotBeTheSameInstanceAs(source.Attributes);
        copy.ToByteArray().Should().SequenceEqual(bytes);
    }
    [Test]
    public void Convert_ImageFile_Null() =>
        AssertThat.Invoking(() => new ImageToScrConverter(BmpFormat.Instance).Convert(null!)).Should().Throw<ArgumentNullException>();
    [TestCase(255, 192)]
    [TestCase(256, 191)]
    public void Convert_ImageFile_InvalidDimensions(int width, int height)
    {
        var source = new BmpFile(new ColourImageData(width, height, Enumerable.Repeat(new Colour(0, 0, 0), width * height).ToArray()));
        AssertThat.Invoking(() => new ImageToScrConverter(source.Format).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_ImageFile_InvalidColour()
    {
        var source = new BmpFile(new ColourImageData(256, 192, Enumerable.Repeat(new Colour(128, 0, 0), 256 * 192).ToArray()));
        AssertThat.Invoking(() => new ImageToScrConverter(source.Format).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_IOFile()
    {
        IOFileConverter converter = new ImageToScrConverter(ScrFormat.Instance);
        var bytes = ScrTestData.Bytes();
        converter.Convert(new ScrFile(bytes)).Should().BeOfType<ScrFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }
    [Test]
    public void Convert_Registered()
    {
        var pixels = Enumerable.Repeat(new Colour(255, 255, 0), 256 * 192).ToArray();
        var source = new BmpFile(new ColourImageData(256, 192, pixels));
        var file = IOFileConversion.Convert<ScrFile>(source);
        file.Attributes.Data.Should().SequenceEqual(Enumerable.Repeat((byte)118, 768));
        var bmp = IOFileConversion.Convert<BmpFile>(file);
        bmp.Image.Should().BeOfType<IndexedImageData>().Value.Palette.Colours.Should().SequenceEqual(ScrTestData.Colours);
        bmp.Image.GetPixel(255, 191).Should().Equal(new Colour(255, 255, 0));
        IOFileConversion.Convert<ScrFile>(bmp).ToByteArray().Should().SequenceEqual(file.ToByteArray());
        IOFileConversion.Convert<ScrFile>(file).ToByteArray().Should().SequenceEqual(file.ToByteArray());
    }
}