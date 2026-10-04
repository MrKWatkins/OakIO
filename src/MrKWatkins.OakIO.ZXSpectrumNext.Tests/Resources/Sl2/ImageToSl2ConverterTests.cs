using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Sl2;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;
using MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Sl2;

using MrKWatkins.OakIO.Resources.Bmp;

public sealed class ImageToSl2ConverterTests
{
    [TestCase("Standard", (ScreenPaletteEncoding)0, false)]
    [TestCase("Standard", (ScreenPaletteEncoding)0, true)]
    [TestCase("Standard", (ScreenPaletteEncoding)1, false)]
    [TestCase("Standard", (ScreenPaletteEncoding)1, true)]
    [TestCase("Standard", (ScreenPaletteEncoding)2, false)]
    [TestCase("Standard", (ScreenPaletteEncoding)2, true)]
    [TestCase("Wide", (ScreenPaletteEncoding)0, false)]
    [TestCase("Wide", (ScreenPaletteEncoding)0, true)]
    [TestCase("Wide", (ScreenPaletteEncoding)1, false)]
    [TestCase("Wide", (ScreenPaletteEncoding)1, true)]
    [TestCase("Wide", (ScreenPaletteEncoding)2, false)]
    [TestCase("Wide", (ScreenPaletteEncoding)2, true)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)0, false)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)0, true)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)1, false)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)1, true)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)2, false)]
    [TestCase("HighResolution", (ScreenPaletteEncoding)2, true)]
    public void Convert_ImageFile(string mode, ScreenPaletteEncoding encoding, bool header)
    {
        var target = Sl2FileTests.Mode(mode);
        var image = ScreenTestData.Image(target.Width, target.Height, target.BitsPerPixel);
        var source = new BmpFile(image);
        var converter = new ImageToSl2Converter(BmpFormat.Instance, target, encoding, header);
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
        AssertThat.Invoking(() => new ImageToSl2Converter(null!, Sl2Format.Standard)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_NullTarget() =>
        AssertThat.Invoking(() => new ImageToSl2Converter(BmpFormat.Instance, null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_InvalidEncoding() =>
        AssertThat.Invoking(() => new ImageToSl2Converter(BmpFormat.Instance, Sl2Format.Standard, (ScreenPaletteEncoding)3)).Should().Throw<ArgumentOutOfRangeException>();

    [Test]
    public void Convert_ImageFile_Null() =>
        AssertThat.Invoking(() => new ImageToSl2Converter(BmpFormat.Instance, Sl2Format.Standard).Convert(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert_ImageFile_Rgb() =>
        AssertThat.Invoking(() => new ImageToSl2Converter(BmpFormat.Instance, Sl2Format.Standard)
            .Convert(new BmpFile(new ColourImageData(1, 1, [new Colour(0, 0, 0)])))).Should().Throw<NotSupportedException>();
}