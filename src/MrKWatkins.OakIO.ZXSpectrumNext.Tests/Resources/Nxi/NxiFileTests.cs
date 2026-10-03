using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxi;

public sealed class NxiFileTests
{
    [TestCase(256, 192, 8)]
    [TestCase(320, 256, 8)]
    [TestCase(640, 256, 4)]
    public void Constructor_ByteArray(int width, int height, int bits)
    {
        var bytes = NxiTestData.Bytes(width, height);
        var expected = bytes.ToArray();
        var file = new NxiFile(bytes);
        file.Format.Should().BeTheSameInstanceAs(NxiFormat.Instance);
        file.Palette.Entries.Count.Should().Equal(1 << bits);
        file.Palette.ToByteArray().Should().SequenceEqual(expected.Take((1 << bits) * 2));
        file.PixelData.Data.Should().SequenceEqual(expected.Skip((1 << bits) * 2));
        file.ToByteArray().Should().SequenceEqual(expected);
        bytes[0] ^= 255;
        bytes[^1] ^= 255;
        file.ToByteArray().Should().SequenceEqual(expected);
    }

    [TestCase(256, 192, 8)]
    [TestCase(320, 256, 8)]
    [TestCase(640, 256, 4)]
    public void Image(int width, int height, int bits)
    {
        var bytes = NxiTestData.Bytes(width, height);
        var file = new NxiFile(bytes);
        var image = file.Image.Should().BeOfType<IndexedImageData>().Value;
        image.Width.Should().Equal(width);
        image.Height.Should().Equal(height);
        image.BitsPerPixel.Should().Equal(bits);
        image.Palette.Colours.Should().SequenceEqual(NxiTestData.Colours(bytes, width));
        image.Pixels.Should().SequenceEqual(NxiTestData.Pixels(bytes, width, height));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public void Constructor_IndexedImageData(int width, int height)
    {
        var bytes = NxiTestData.Bytes(width, height);
        var file = new NxiFile(NxiTestData.Image(width, height));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void Constructor_IndexedImageData_Null() =>
        AssertThat.Invoking(() => new NxiFile(image: null!)).Should().Throw<ArgumentNullException>();

    [TestCase(255, 192)]
    [TestCase(257, 192)]
    [TestCase(256, 191)]
    [TestCase(256, 193)]
    [TestCase(320, 255)]
    [TestCase(640, 257)]
    public void Constructor_IndexedImageData_InvalidDimensions(int width, int height)
    {
        var image = new IndexedImageData(width, height, new byte[width * height], new PaletteData([new Colour(0, 0, 0)]));
        AssertThat.Invoking(() => new NxiFile(image)).Should().Throw<ArgumentException>();
    }

    [TestCase(256, 192, 1)]
    [TestCase(256, 192, 255)]
    [TestCase(320, 256, 16)]
    [TestCase(640, 256, 15)]
    [TestCase(640, 256, 17)]
    [TestCase(640, 256, 256)]
    public void Constructor_IndexedImageData_InvalidPaletteCount(int width, int height, int count)
    {
        var image = new IndexedImageData(width, height, new byte[width * height], new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0), count)));
        AssertThat.Invoking(() => new NxiFile(image)).Should().Throw<ArgumentException>();
    }

    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public void Constructor_IndexedImageData_Alpha(int width, int height)
    {
        var colours = NxiTestData.Colours(NxiTestData.Bytes(width, height), width);
        colours[^1] = new Colour(0, 0, 0, 128);
        var image = new IndexedImageData(width, height, new byte[width * height], new PaletteData(colours));
        AssertThat.Invoking(() => new NxiFile(image)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_IndexedImageData_RoundingAndDuplicates()
    {
        var colours = Enumerable.Repeat(new Colour(128, 36, 255), 16).ToArray();
        var pixels = new byte[640 * 256];
        pixels[1] = 15;
        var file = new NxiFile(new IndexedImageData(640, 256, pixels, new PaletteData(colours)));
        file.Palette.ToByteArray().Should().SequenceEqual(Enumerable.Repeat(new byte[] { 135, 1 }, 16).SelectMany(entry => entry));
        file.Image.Should().BeOfType<IndexedImageData>().Value.Palette.Colours.Should().SequenceEqual(Enumerable.Repeat(new Colour(146, 36, 255), 16));
        file.PixelData.Data[0].Should().Equal((byte)15);
        file.PixelData.Pixels[1].Should().Equal((byte)15);
    }

    [TestCase(0)]
    [TestCase(49152)]
    [TestCase(49663)]
    [TestCase(49665)]
    [TestCase(81920)]
    [TestCase(81951)]
    [TestCase(81953)]
    [TestCase(82431)]
    [TestCase(82433)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new NxiFile(new byte[length])).Should().Throw<InvalidDataException>();

    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public void Constructor_ByteArray_InvalidPaletteEntry(int width, int height)
    {
        var bytes = NxiTestData.Bytes(width, height);
        bytes[width == 640 ? 31 : 511] = 2;
        AssertThat.Invoking(() => new NxiFile(bytes)).Should().Throw<InvalidDataException>();
    }
}