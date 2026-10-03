using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ColourImageDataTests
{
    [TestCase(-1, 0, "x")]
    [TestCase(2, 0, "x")]
    [TestCase(3, 0, "x")]
    [TestCase(0, -1, "y")]
    [TestCase(0, 2, "y")]
    [TestCase(0, 3, "y")]
    public void GetPixel_InvalidCoordinates(int x, int y, string parameter) =>
        AssertThat.Invoking(() => new ColourImageData(2, 2, [default, default, default, default]).GetPixel(x, y)).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal(parameter);

    [Test]
    public void Constructor()
    {
        Colour[] pixels = [new(1, 2, 3), new(4, 5, 6, 0), new(7, 8, 9), new(10, 11, 12)];
        var image = new ColourImageData(2, 2, pixels);
        image.Pixels.Should().SequenceEqual(pixels);
        pixels[0] = default;
        image.Pixels[0].Should().Equal(new Colour(1, 2, 3));
        AssertThat.Invoking(() => ((IList<Colour>)image.Pixels)[0] = default).Should().Throw<NotSupportedException>();
    }

    [TestCase(0)]
    [TestCase(3)]
    [TestCase(5)]
    public void Constructor_InvalidPixelCount(int count) =>
        AssertThat.Invoking(() => new ColourImageData(2, 2, new Colour[count])).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("pixels");

    [Test]
    public void Constructor_NullPixels() =>
        AssertThat.Invoking(() => new ColourImageData(1, 1, null!)).Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("pixels");

    [TestCase(0, 0, 1)]
    [TestCase(1, 0, 2)]
    [TestCase(0, 1, 3)]
    [TestCase(1, 1, 4)]
    public void GetPixel(int x, int y, byte red)
    {
        var image = new ColourImageData(2, 2, [new(1, 0, 0, 0), new(2, 0, 0, 0), new(3, 0, 0, 0), new(4, 0, 0, 0)]);
        image.GetPixel(x, y).Should().Equal(new Colour(red, 0, 0, 0));
    }
}