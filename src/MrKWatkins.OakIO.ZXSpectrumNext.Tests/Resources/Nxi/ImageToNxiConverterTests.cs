using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxi;

public sealed class ImageToNxiConverterTests
{
    [Test]
    public void Constructor()
    {
        var converter = new ImageToNxiConverter(BmpFormat.Instance);
        converter.SourceFormat.Should().BeTheSameInstanceAs(BmpFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(NxiFormat.Instance);
    }

    [Test]
    public void Constructor_Null() =>
        AssertThat.Invoking(() => new ImageToNxiConverter(null!)).Should().Throw<ArgumentNullException>();

    [TestCase(256, 192, false)]
    [TestCase(320, 256, true)]
    [TestCase(640, 256, false)]
    public void Convert_ImageFile(int width, int height, bool topDown)
    {
        var source = new BmpFile(NxiTestData.Image(width, height), isTopDown: topDown);
        var file = new ImageToNxiConverter(source.Format).Convert(source);
        file.ToByteArray().Should().SequenceEqual(NxiTestData.Bytes(width, height));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Convert_ImageFile_FourBit(bool topDown)
    {
        var source = new BmpFile(NxiTestData.Image(640, 256, 4), isTopDown: topDown);
        var file = new ImageToNxiConverter(source.Format).Convert(source);
        file.PixelData.BitsPerPixel.Should().Equal(4);
        file.ToByteArray().Should().SequenceEqual(NxiTestData.Bytes(640, 256));
    }

    [Test]
    public void Convert_ImageFile_Nxi()
    {
        var bytes = NxiTestData.Bytes(640, 256);
        var source = NxiFormat.Instance.Read(bytes);
        var copy = new ImageToNxiConverter(source.Format).Convert(source);
        copy.Should().NotBeTheSameInstanceAs(source);
        copy.Palette.Should().NotBeTheSameInstanceAs(source.Palette);
        copy.PixelData.Should().NotBeTheSameInstanceAs(source.PixelData);
        copy.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void Convert_ImageFile_Null() =>
        AssertThat.Invoking(() => new ImageToNxiConverter(BmpFormat.Instance).Convert(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert_ImageFile_NonIndexed()
    {
        var source = new BmpFile(new ColourImageData(256, 192, Enumerable.Repeat(new Colour(0, 0, 0), 256 * 192).ToArray()));
        AssertThat.Invoking(() => new ImageToNxiConverter(source.Format).Convert(source)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Convert_ImageFile_InvalidDimensions()
    {
        var source = new BmpFile(new IndexedImageData(1, 1, [0], new PaletteData([new Colour(0, 0, 0)])));
        AssertThat.Invoking(() => new ImageToNxiConverter(source.Format).Convert(source)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Convert_ImageFile_InvalidPaletteCount()
    {
        var source = new BmpFile(new IndexedImageData(256, 192, new byte[256 * 192], new PaletteData([new Colour(0, 0, 0)])));
        AssertThat.Invoking(() => new ImageToNxiConverter(source.Format).Convert(source)).Should().Throw<ArgumentException>();
    }

    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public void Convert_Registered(int width, int height)
    {
        var source = new BmpFile(NxiTestData.Image(width, height));
        var file = IOFileConversion.Convert<NxiFile>(source);
        file.ToByteArray().Should().SequenceEqual(NxiTestData.Bytes(width, height));
        var bmp = IOFileConversion.Convert<BmpFile>(file);
        var image = bmp.Image.Should().BeOfType<IndexedImageData>().Value;
        image.Pixels.Should().SequenceEqual(NxiTestData.Pixels(NxiTestData.Bytes(width, height), width, height));
        image.Palette.Colours.Should().SequenceEqual(NxiTestData.Colours(NxiTestData.Bytes(width, height), width));
        IOFileConversion.Convert<NxiFile>(bmp).ToByteArray().Should().SequenceEqual(file.ToByteArray());
        IOFileConversion.Convert<NxiFile>(file).ToByteArray().Should().SequenceEqual(file.ToByteArray());
    }

    [Test]
    public void Convert_IOFile()
    {
        IOFileConverter converter = new ImageToNxiConverter(NxiFormat.Instance);
        var bytes = NxiTestData.Bytes(256, 192);
        converter.Convert(NxiFormat.Instance.Read(bytes)).Should().BeOfType<NxiFile>().Value.ToByteArray().Should().SequenceEqual(bytes);
    }
}