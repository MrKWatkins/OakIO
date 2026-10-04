using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

public sealed class ScreenPaletteTests
{
    [TestCase(16)]
    [TestCase(256)]
    public void Constructor_ByteArray(int count)
    {
        var defaults = new ScreenPalette([], count);
        defaults.Count.Should().Equal(count);
        defaults.Encoding.Should().Equal(ScreenPaletteEncoding.None);
        defaults.Length.Should().Equal(0);
        defaults.Palette.Colours[0].Should().Equal(new Colour(0, 0, 0));
        defaults.Palette.Colours[1].Should().Equal(new Colour(0, 0, 109));
        defaults.Palette.Colours[2].Should().Equal(new Colour(0, 0, 182));
        defaults.Palette.Colours[3].Should().Equal(new Colour(0, 0, 255));
        var bytes = Enumerable.Range(0, count).Select(index => (byte)index).ToArray();
        var palette = new ScreenPalette(bytes, count);
        palette.Palette.Colours.Should().SequenceEqual(defaults.Palette.Colours);
        palette.GetPriority(count - 1).Should().BeFalse();
        bytes[0] = 255;
        palette.Data[0].Should().Equal((byte)0);
    }

    [Test]
    public void Constructor_ByteArray_Rgb333()
    {
        var bytes = new byte[32];
        bytes[0] = 255;
        bytes[1] = 129;
        var palette = new ScreenPalette(bytes, 16);
        palette.Palette.Colours[0].Should().Equal(new Colour(255, 255, 255));
        palette.Palette.Colours[1].Should().Equal(new Colour(0, 0, 0));
        palette.GetPriority(0).Should().BeTrue();
        palette.GetPriority(1).Should().BeFalse();
        palette.Data.Should().SequenceEqual(bytes);
    }

    [TestCase(1)]
    [TestCase(15)]
    [TestCase(17)]
    [TestCase(31)]
    [TestCase(33)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new ScreenPalette(new byte[length], 16)).Should().Throw<InvalidDataException>();

    [TestCase(2)]
    [TestCase(126)]
    [TestCase(255)]
    public void Constructor_ByteArray_ReservedBits(byte value)
    {
        var bytes = new byte[32];
        bytes[31] = value;
        AssertThat.Invoking(() => new ScreenPalette(bytes, 16)).Should().Throw<InvalidDataException>();
    }

    [TestCase(ScreenPaletteEncoding.None)]
    [TestCase(ScreenPaletteEncoding.Rgb332)]
    [TestCase(ScreenPaletteEncoding.Rgb333)]
    public void Create(ScreenPaletteEncoding encoding)
    {
        var original = new ScreenPalette([], 256).Palette;
        var palette = ScreenPalette.Create(original, 256, encoding);
        palette.Encoding.Should().Equal(encoding);
        palette.Palette.Colours.Should().SequenceEqual(original.Colours);
        palette.Length.Should().Equal(256 * (int)encoding);
    }

    [Test]
    public void Create_Rounding()
    {
        var colours = Enumerable.Repeat(new Colour(128, 128, 128), 16).ToArray();
        var source = new PaletteData(colours);
        ScreenPalette.Create(source, 16, ScreenPaletteEncoding.Rgb333).Data.Take(2).Should().SequenceEqual(146, 0);
        ScreenPalette.Create(source, 16, ScreenPaletteEncoding.Rgb332).Data[0].Should().Equal((byte)145);
    }

    [Test]
    public void Create_Null() =>
        AssertThat.Invoking(() => ScreenPalette.Create(null!, 16, ScreenPaletteEncoding.None)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Create_InvalidCount() =>
        AssertThat.Invoking(() => ScreenPalette.Create(new PaletteData([new Colour()]), 16, ScreenPaletteEncoding.None)).Should().Throw<ArgumentException>();

    [Test]
    public void Create_InvalidEncoding() =>
        AssertThat.Invoking(() => ScreenPalette.Create(new ScreenPalette([], 16).Palette, 16, (ScreenPaletteEncoding)3)).Should().Throw<ArgumentOutOfRangeException>();

    [TestCase(ScreenPaletteEncoding.None)]
    [TestCase(ScreenPaletteEncoding.Rgb332)]
    [TestCase(ScreenPaletteEncoding.Rgb333)]
    public void Create_Alpha(ScreenPaletteEncoding encoding) =>
        AssertThat.Invoking(() => ScreenPalette.Create(new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0, 254), 16)), 16, encoding)).Should().Throw<ArgumentException>();

    [Test]
    public void Create_NonDefaultPalette() =>
        AssertThat.Invoking(() => ScreenPalette.Create(new PaletteData(Enumerable.Repeat(new Colour(255, 255, 255), 16)), 16, ScreenPaletteEncoding.None)).Should().Throw<ArgumentException>();

    [TestCase(-1)]
    [TestCase(16)]
    public void GetPriority_InvalidIndex(int index) =>
        AssertThat.Invoking(() => new ScreenPalette([], 16).GetPriority(index)).Should().Throw<ArgumentOutOfRangeException>();
}