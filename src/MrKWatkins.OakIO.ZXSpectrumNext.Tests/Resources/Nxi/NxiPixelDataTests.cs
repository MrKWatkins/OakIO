using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxi;

public sealed class NxiPixelDataTests
{
    [TestCase(256, 192, 8)]
    [TestCase(320, 256, 8)]
    [TestCase(640, 256, 4)]
    public void Constructor_ByteArray(int width, int height, int bits)
    {
        var bytes = NxiTestData.Bytes(width, height);
        var raw = bytes.Skip((1 << bits) * 2).ToArray();
        var data = new NxiPixelData(raw, width, height);
        data.Width.Should().Equal(width);
        data.Height.Should().Equal(height);
        data.BitsPerPixel.Should().Equal(bits);
        data.Length.Should().Equal(width * height * bits / 8);
        data.Data.Should().SequenceEqual(bytes.Skip((1 << bits) * 2));
        data.Pixels.Should().SequenceEqual(NxiTestData.Pixels(bytes, width, height));
        raw[0] ^= 255;
        data.Data.Should().SequenceEqual(bytes.Skip((1 << bits) * 2));
    }

    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public void Constructor_IndexedImageData(int width, int height)
    {
        var bytes = NxiTestData.Bytes(width, height);
        var data = new NxiPixelData(NxiTestData.Image(width, height));
        data.Data.Should().SequenceEqual(bytes.Skip(width == 640 ? 32 : 512));
        data.Pixels.Should().SequenceEqual(NxiTestData.Pixels(bytes, width, height));
    }

    [TestCase(256, 192, 0)]
    [TestCase(256, 192, 49151)]
    [TestCase(256, 192, 49153)]
    [TestCase(320, 256, 81919)]
    [TestCase(320, 256, 81921)]
    [TestCase(640, 256, 81919)]
    [TestCase(640, 256, 81921)]
    public void Constructor_ByteArray_InvalidLength(int width, int height, int length) =>
        AssertThat.Invoking(() => new NxiPixelData(new byte[length], width, height)).Should().Throw<ArgumentException>();

    [TestCase(255, 192)]
    [TestCase(256, 191)]
    [TestCase(320, 192)]
    [TestCase(640, 192)]
    public void Constructor_ByteArray_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new NxiPixelData([], width, height)).Should().Throw<ArgumentException>();

    [Test]
    public void Constructor_IndexedImageData_Null() =>
        AssertThat.Invoking(() => new NxiPixelData(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_IndexedImageData_InvalidIndex()
    {
        var pixels = new byte[640 * 256];
        pixels[^1] = 16;
        var image = new IndexedImageData(640, 256, pixels, new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0), 17)));
        AssertThat.Invoking(() => new NxiPixelData(image)).Should().Throw<ArgumentException>();
    }

    [TestCase(320, 0, 0, 0, 0xA5, 165)]
    [TestCase(320, 0, 255, 255, 0xA5, 165)]
    [TestCase(320, 1, 0, 256, 0xA5, 165)]
    [TestCase(320, 319, 255, 81919, 0xA5, 165)]
    [TestCase(640, 0, 0, 0, 0xA5, 10)]
    [TestCase(640, 1, 0, 0, 0xA5, 5)]
    [TestCase(640, 2, 0, 256, 0xA5, 10)]
    [TestCase(640, 639, 255, 81919, 0xA5, 5)]
    public void Pixels(int width, int x, int y, int offset, byte value, byte expected)
    {
        var bytes = new byte[81920];
        bytes[offset] = value;
        new NxiPixelData(bytes, width, 256).Pixels[y * width + x].Should().Equal(expected);
    }
}