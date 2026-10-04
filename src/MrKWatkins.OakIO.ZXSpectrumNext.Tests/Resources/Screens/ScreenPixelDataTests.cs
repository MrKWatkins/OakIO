using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

public sealed class ScreenPixelDataTests
{
    [TestCase(256, 192, 8)]
    [TestCase(320, 256, 8)]
    [TestCase(640, 256, 4)]
    [TestCase(128, 96, 8)]
    [TestCase(128, 96, 4)]
    public void Create(int width, int height, int bits)
    {
        var image = ScreenTestData.Image(width, height, bits);
        var component = ScreenPixelData.Create(image, width, height, bits);
        component.Pixels.Should().SequenceEqual(image.Pixels);
        component.Width.Should().Equal(width);
        component.Height.Should().Equal(height);
        component.BitsPerPixel.Should().Equal(bits);
        component.Length.Should().Equal(width * height * bits / 8);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x += 8 / bits)
            {
                var expected = bits == 8 ? image.Pixels[y * width + x] :
                    (byte)((image.Pixels[y * width + x] << 4) | image.Pixels[y * width + x + 1]);
                var offset = width is 320 or 640 ? x / (8 / bits) * height + y : (y * width + x) / (8 / bits);
                component.Data[offset].Should().Equal(expected);
            }
        }
    }

    [TestCase(256, 192, 8)]
    [TestCase(320, 256, 8)]
    [TestCase(640, 256, 4)]
    [TestCase(128, 96, 8)]
    [TestCase(128, 96, 4)]
    public void Constructor_ByteArray(int width, int height, int bits)
    {
        var bytes = new byte[width * height * bits / 8];
        bytes[0] = 0xAB;
        bytes[1] = 0xCD;
        bytes[^1] = 0xEF;
        var data = new ScreenPixelData(bytes, width, height, bits);
        var pixels = data.Pixels;
        pixels[0].Should().Equal(bits == 8 ? (byte)0xAB : (byte)0xA);
        if (width is 320 or 640)
        {
            pixels[width].Should().Equal(bits == 8 ? (byte)0xCD : (byte)0xC);
        }
        else
        {
            pixels[8 / bits].Should().Equal(bits == 8 ? (byte)0xCD : (byte)0xC);
        }
        pixels[^1].Should().Equal(bits == 8 ? (byte)0xEF : (byte)0xF);
        bytes[0] = 0;
        data.Data[0].Should().Equal((byte)0xAB);
    }

    [Test]
    public void Create_Null() =>
        AssertThat.Invoking(() => ScreenPixelData.Create(null!, 128, 96, 8)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Create_InvalidDimensions() =>
        AssertThat.Invoking(() => ScreenPixelData.Create(ScreenTestData.Image(128, 96, 8), 256, 192, 8)).Should().Throw<ArgumentException>();

    [Test]
    public void Create_InvalidIndex()
    {
        var pixels = new byte[128 * 96];
        pixels[^1] = 16;
        var image = new IndexedImageData(128, 96, pixels, new PaletteData(Enumerable.Repeat(new Colour(), 17)));
        AssertThat.Invoking(() => ScreenPixelData.Create(image, 128, 96, 4)).Should().Throw<ArgumentException>();
    }
}