using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ImageDataTests
{
    [Test]
    public void Constructor()
    {
        var image = new TestImageData(3, 2);
        image.Width.Should().Equal(3);
        image.Height.Should().Equal(2);
        image.PixelCount.Should().Equal(6);
    }

    [TestCase(0, 1, "width")]
    [TestCase(-1, 1, "width")]
    [TestCase(1, 0, "height")]
    [TestCase(1, -1, "height")]
    public void Constructor_InvalidDimensions(int width, int height, string parameter) =>
        AssertThat.Invoking(() => new TestImageData(width, height)).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal(parameter);

    [Test]
    public void Constructor_Overflow() => AssertThat.Invoking(() => new TestImageData(int.MaxValue, 2)).Should().Throw<OverflowException>();

    [TestCase(0, 0, 0)]
    [TestCase(2, 0, 2)]
    [TestCase(0, 1, 3)]
    [TestCase(1, 1, 4)]
    [TestCase(2, 1, 5)]
    public void GetPixelOffset(int x, int y, int expected) => new TestImageData(3, 2).Offset(x, y).Should().Equal(expected);

    [TestCase(-1, 0, "x")]
    [TestCase(3, 0, "x")]
    [TestCase(0, -1, "y")]
    [TestCase(0, 2, "y")]
    public void GetPixelOffset_InvalidCoordinates(int x, int y, string parameter) =>
        AssertThat.Invoking(() => new TestImageData(3, 2).Offset(x, y)).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal(parameter);

    private sealed class TestImageData(int width, int height) : ImageData(width, height)
    {
        public int Offset(int x, int y) => GetPixelOffset(x, y);
        public override Colour GetPixel(int x, int y) => new((byte)GetPixelOffset(x, y), 0, 0);
    }
}