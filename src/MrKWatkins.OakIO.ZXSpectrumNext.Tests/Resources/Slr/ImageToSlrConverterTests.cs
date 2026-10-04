using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Slr;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;
using MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Slr;

using MrKWatkins.OakIO.Resources.Bmp;

public sealed class ImageToSlrConverterTests
{
    [TestCase("EightBit", (ScreenPaletteEncoding)0, false)]
    [TestCase("EightBit", (ScreenPaletteEncoding)0, true)]
    [TestCase("EightBit", (ScreenPaletteEncoding)1, false)]
    [TestCase("EightBit", (ScreenPaletteEncoding)1, true)]
    [TestCase("EightBit", (ScreenPaletteEncoding)2, false)]
    [TestCase("EightBit", (ScreenPaletteEncoding)2, true)]
    [TestCase("FourBit", (ScreenPaletteEncoding)0, false)]
    [TestCase("FourBit", (ScreenPaletteEncoding)0, true)]
    [TestCase("FourBit", (ScreenPaletteEncoding)1, false)]
    [TestCase("FourBit", (ScreenPaletteEncoding)1, true)]
    [TestCase("FourBit", (ScreenPaletteEncoding)2, false)]
    [TestCase("FourBit", (ScreenPaletteEncoding)2, true)]
    public void Convert_ImageFile(string mode, ScreenPaletteEncoding encoding, bool header)
    {
        var target = SlrFileTests.Mode(mode);
        var image = ScreenTestData.Image(target.Width, target.Height, target.BitsPerPixel);
        var source = new BmpFile(image);
        var converter = new ImageToSlrConverter(BmpFormat.Instance, target, encoding, header);
        converter.SourceFormat.Should().BeTheSameInstanceAs(BmpFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(target);
        var result = converter.Convert(source);
        result.Image.Should().BeOfType<IndexedImageData>().Value.Pixels.Should().SequenceEqual(image.Pixels);
        result.Palette.Encoding.Should().Equal(encoding);
        (result.Header != null).Should().Equal(header);
        var bmp = new ImageToBmpConverter(target).Convert(result);
        bmp.Image.Should().BeOfType<IndexedImageData>().Value.Pixels.Should().SequenceEqual(image.Pixels);
    }

    [Test]
    public void Constructor_NullSource() =>
        AssertThat.Invoking(() => new ImageToSlrConverter(null!, SlrFormat.EightBit)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_NullTarget() =>
        AssertThat.Invoking(() => new ImageToSlrConverter(BmpFormat.Instance, null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_InvalidEncoding() =>
        AssertThat.Invoking(() => new ImageToSlrConverter(BmpFormat.Instance, SlrFormat.EightBit, (ScreenPaletteEncoding)3)).Should().Throw<ArgumentOutOfRangeException>();

    [Test]
    public void Convert_ImageFile_Null() =>
        AssertThat.Invoking(() => new ImageToSlrConverter(BmpFormat.Instance, SlrFormat.EightBit).Convert(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert_ImageFile_Rgb() =>
        AssertThat.Invoking(() => new ImageToSlrConverter(BmpFormat.Instance, SlrFormat.EightBit)
            .Convert(new BmpFile(new ColourImageData(1, 1, [new Colour(0, 0, 0)])))).Should().Throw<NotSupportedException>();
}