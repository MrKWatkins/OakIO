using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources.Scr;

public sealed class ScrFileTests
{
    [Test]
    public void Constructor_ByteArray()
    {
        var bytes = ScrTestData.Bytes();
        var expected = bytes.ToArray();
        var file = new ScrFile(bytes);
        file.Format.Should().BeTheSameInstanceAs(ScrFormat.Instance);
        file.Bitmap.Data.Should().SequenceEqual(expected.Take(6144));
        file.Attributes.Data.Should().SequenceEqual(expected.Skip(6144));
        file.ToByteArray().Should().SequenceEqual(expected);
        bytes[0] ^= 255;
        bytes[6144] ^= 255;
        file.ToByteArray().Should().SequenceEqual(expected);
    }
    [Test]
    public void Constructor_ByteArray_Null() =>
        AssertThat.Invoking(() => new ScrFile(data: null!)).Should().Throw<ArgumentNullException>();
    [TestCase(0)]
    [TestCase(6144)]
    [TestCase(6911)]
    [TestCase(6913)]
    [TestCase(12288)]
    [TestCase(12289)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new ScrFile(new byte[length])).Should().Throw<InvalidDataException>();

    [TestCase(false)]
    [TestCase(true)]
    public void Render(bool flashPhase)
    {
        var bytes = ScrTestData.Bytes();
        var file = new ScrFile(bytes);
        var image = file.Render(flashPhase);
        image.Width.Should().Equal(256);
        image.Height.Should().Equal(192);
        image.BitsPerPixel.Should().Equal(4);
        image.Palette.Colours.Should().SequenceEqual(ScrTestData.Colours);
        image.Pixels.Should().SequenceEqual(ScrTestData.Pixels(bytes, flashPhase));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }
    [Test]
    public void Image()
    {
        var bytes = ScrTestData.Bytes();
        new ScrFile(bytes).Image.Should().BeOfType<IndexedImageData>().Value.Pixels.Should().SequenceEqual(ScrTestData.Pixels(bytes, false));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Constructor_ImageData(bool bright, bool blackIsPaper)
    {
        var colour = bright ? new Colour(255, 0, 0) : new Colour(192, 0, 0);
        var black = new Colour(0, 0, 0);
        var paper = blackIsPaper ? black : colour;
        var ink = blackIsPaper ? colour : black;
        var pixels = Enumerable.Repeat(paper, 256 * 192).ToArray();
        // Interior cell at (2, 8), across the boundary into the middle third; first and last bits in its second scan.
        pixels[65 * 256 + 16] = ink;
        pixels[65 * 256 + 23] = ink;
        var file = new ScrFile(new ColourImageData(256, 192, pixels));
        file.Bitmap.Data[2306].Should().Equal((byte)129);
        file.Bitmap.Data.Sum(value => value).Should().Equal(129);
        file.Attributes.GetAttribute(2, 8).Value.Should().Equal((byte)((bright ? 64 : 0) | (blackIsPaper ? 2 : 16)));
        var rendered = file.Render();
        rendered.Pixels.Select(index => rendered.Palette.Colours[index]).Should().SequenceEqual(pixels);
    }
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(9)]
    [TestCase(15)]
    public void Constructor_ImageData_Solid(int colourIndex)
    {
        var colour = ScrTestData.Colours[colourIndex];
        var image = new ColourImageData(256, 192, Enumerable.Repeat(colour, 256 * 192).ToArray());
        var file = new ScrFile(image);
        file.Bitmap.Data.Should().SequenceEqual(new byte[6144]);
        file.Attributes.Data.Should().SequenceEqual(Enumerable.Repeat((byte)((colourIndex == 8 ? 0 : colourIndex >= 8 ? 64 : 0) | (colourIndex % 8 * 9)), 768));
        file.Render().Pixels.Select(index => ScrTestData.Colours[index]).Should().SequenceEqual(Enumerable.Repeat(colour, 256 * 192));
    }
    [Test]
    public void Constructor_ImageData_Null() =>
        AssertThat.Invoking(() => new ScrFile(image: null!)).Should().Throw<ArgumentNullException>();
    [TestCase(255, 192)]
    [TestCase(257, 192)]
    [TestCase(256, 191)]
    [TestCase(256, 193)]
    public void Constructor_ImageData_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new ScrFile(new ColourImageData(width, height, new Colour[width * height]))).Should().Throw<ArgumentException>();
    [TestCase(1, 0, 0, 255)]
    [TestCase(205, 0, 0, 255)]
    [TestCase(192, 255, 0, 255)]
    [TestCase(0, 0, 0, 254)]
    [TestCase(0, 0, 0, 0)]
    public void Constructor_ImageData_InvalidColour(byte red, byte green, byte blue, byte alpha)
    {
        var pixels = Enumerable.Repeat(new Colour(0, 0, 0), 256 * 192).ToArray();
        pixels[^1] = new Colour(red, green, blue, alpha);
        AssertThat.Invoking(() => new ScrFile(new ColourImageData(256, 192, pixels))).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Constructor_ImageData_ThreeColours()
    {
        var pixels = Enumerable.Repeat(new Colour(0, 0, 0), 256 * 192).ToArray();
        pixels[1] = new Colour(192, 0, 0);
        pixels[2] = new Colour(0, 192, 0);
        AssertThat.Invoking(() => new ScrFile(new ColourImageData(256, 192, pixels))).Should().Throw<ArgumentException>();
    }
    [TestCase(false)]
    [TestCase(true)]
    public void Constructor_ImageData_MixedBrightness(bool reverse)
    {
        var pixels = Enumerable.Repeat(new Colour(192, 0, 0), 256 * 192).ToArray();
        pixels[reverse ? 0 : 1] = new Colour(0, 255, 0);
        AssertThat.Invoking(() => new ScrFile(new ColourImageData(256, 192, pixels))).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Constructor_ImageData_SeparateCells()
    {
        var pixels = Enumerable.Repeat(new Colour(192, 0, 0), 256 * 192).ToArray();
        for (var y = 0; y < 8; y++)
        {
            for (var x = 8; x < 16; x++)
            {
                pixels[y * 256 + x] = new Colour(0, 255, 0);
            }
        }
        var file = new ScrFile(new ColourImageData(256, 192, pixels));
        file.Attributes.GetAttribute(0, 0).Bright.Should().BeFalse();
        file.Attributes.GetAttribute(1, 0).Bright.Should().BeTrue();
        file.Render().GetPixel(7, 0).Should().Equal(new Colour(192, 0, 0));
        file.Render().GetPixel(8, 0).Should().Equal(new Colour(0, 255, 0));
    }
}