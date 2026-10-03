using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class IndexedImageDataTests
{
    [TestCase(-1, 0, "x")]
    [TestCase(2, 0, "x")]
    [TestCase(3, 0, "x")]
    [TestCase(0, -1, "y")]
    [TestCase(0, 2, "y")]
    [TestCase(0, 3, "y")]
    public void GetPixel_InvalidCoordinates(int x, int y, string parameter) =>
        AssertThat.Invoking(() => new IndexedImageData(2, 2, [0, 0, 0, 0], new PaletteData([default])).GetPixel(x, y)).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal(parameter);

    [Test]
    public void Constructor()
    {
        byte[] pixels = [1, 0, 0, 1];
        var palette = new PaletteData([new Colour(10, 20, 30), new Colour(40, 50, 60, 0)]);
        var image = new IndexedImageData(2, 2, pixels, palette);
        image.Palette.Should().BeTheSameInstanceAs(palette);
        image.BitsPerPixel.Should().Equal(8);
        image.Pixels.Should().SequenceEqual(pixels);
        pixels[0] = 0;
        image.Pixels[0].Should().Equal((byte)1);
        AssertThat.Invoking(() => ((IList<byte>)image.Pixels)[0] = 0).Should().Throw<NotSupportedException>();
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor_BitDepth(int bits)
    {
        var count = 1 << bits;
        var palette = new PaletteData(Enumerable.Range(0, count).Select(i => new Colour((byte)i, 20, 30)));
        var image = new IndexedImageData(2, 1, [0, (byte)(count - 1)], palette, bits);
        image.BitsPerPixel.Should().Equal(bits);
        image.GetPixel(0, 0).Should().Equal(new Colour(0, 20, 30));
        image.GetPixel(1, 0).Should().Equal(new Colour((byte)(count - 1), 20, 30));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(7)]
    [TestCase(9)]
    public void Constructor_InvalidBitDepth(int bits) =>
        AssertThat.Invoking(() => new IndexedImageData(1, 1, [0], new PaletteData([default]), bits)).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal("bitsPerPixel");

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor_PaletteTooLarge(int bits) =>
        AssertThat.Invoking(() => new IndexedImageData(1, 1, [0], new PaletteData(Enumerable.Repeat(default(Colour), (1 << bits) + 1)), bits))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("palette");

    [TestCase(0)]
    [TestCase(3)]
    [TestCase(5)]
    public void Constructor_InvalidPixelCount(int count) =>
        AssertThat.Invoking(() => new IndexedImageData(2, 2, new byte[count], new PaletteData([default])))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("pixels");

    [TestCase(1)]
    [TestCase(255)]
    public void Constructor_MissingPaletteEntry(byte index) =>
        AssertThat.Invoking(() => new IndexedImageData(2, 1, [0, index], new PaletteData([default])))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("pixels");

    [Test]
    public void Constructor_NullPixels() =>
        AssertThat.Invoking(() => new IndexedImageData(1, 1, null!, new PaletteData([default])))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("pixels");

    [Test]
    public void Constructor_NullPalette() =>
        AssertThat.Invoking(() => new IndexedImageData(1, 1, [0], null!))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("palette");

    [TestCase(0, 0, 1)]
    [TestCase(1, 0, 0)]
    [TestCase(0, 1, 0)]
    [TestCase(1, 1, 1)]
    public void GetPixel(int x, int y, int index)
    {
        var palette = new PaletteData([new Colour(1, 2, 3), new Colour(4, 5, 6, 0)]);
        new IndexedImageData(2, 2, [1, 0, 0, 1], palette).GetPixel(x, y).Should().Equal(palette.Colours[index]);
    }
}