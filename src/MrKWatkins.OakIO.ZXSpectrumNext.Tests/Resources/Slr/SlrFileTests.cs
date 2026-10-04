using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Slr;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;
using MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Slr;

public sealed class SlrFileTests
{
    internal static SlrFormat Mode(string mode) => mode switch
    {
        "EightBit" => SlrFormat.EightBit,
        "FourBit" => SlrFormat.FourBit,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

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
    public void Constructor_IndexedImageData(string mode, ScreenPaletteEncoding encoding, bool header)
    {
        var format = Mode(mode);
        var image = ScreenTestData.Image(format.Width, format.Height, format.BitsPerPixel);
        var file = new SlrFile(image, format, encoding, header);
        file.Format.Should().BeTheSameInstanceAs(format);
        file.Image.Should().BeOfType<IndexedImageData>().Value.Pixels.Should().SequenceEqual(image.Pixels);
        file.Image.GetPixel(0, 0).Should().Equal(image.GetPixel(0, 0));
        file.Palette.Encoding.Should().Equal(encoding);
        (file.Header != null).Should().Equal(header);
        var bytes = file.ToByteArray();
        bytes.Length.Should().Equal(format.Width * format.Height * format.BitsPerPixel / 8 + (1 << format.BitsPerPixel) * (int)encoding + (header ? 128 : 0));
        var restored = format.Read(bytes);
        restored.ToByteArray().Should().SequenceEqual(bytes);
        restored.Image.Should().BeOfType<IndexedImageData>().Value.Pixels.Should().SequenceEqual(image.Pixels);
        restored.Palette.Palette.Colours.Should().SequenceEqual(image.Palette.Colours);
        bytes[header ? 128 : 0] ^= 255;
        restored.ToByteArray().Should().SequenceEqual(file.ToByteArray());
    }

    [Test]
    public void Constructor_IndexedImageData_Null() =>
        AssertThat.Invoking(() => new SlrFile(null!, SlrFormat.EightBit)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_IndexedImageData_NullFormat() =>
        AssertThat.Invoking(() => new SlrFile(ScreenTestData.Image(128, 96, 8), null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_ByteArray_Null() =>
        AssertThat.Invoking(() => new SlrFile(null!, SlrFormat.EightBit)).Should().Throw<ArgumentNullException>();
}