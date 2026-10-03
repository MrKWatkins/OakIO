using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Tests.Resources.Bmp;

public sealed class BmpFileTests
{
    [Test]
    public void Constructor()
    {
        var image = new ColourImageData(1, 1, [new Colour(1, 2, 3)]);
        var file = new BmpFile(image);
        file.Image.GetPixel(0, 0).Should().Equal(new Colour(1, 2, 3));
        file.Format.Should().BeTheSameInstanceAs(BmpFormat.Instance);
        file.BitsPerPixel.Should().Equal(24);
        file.IsTopDown.Should().BeFalse();
    }

    [TestCase(4)]
    [TestCase(8)]
    public void Constructor_Indexed(int depth)
    {
        var image = new IndexedImageData(1, 1, [0], new PaletteData([new Colour(1, 2, 3)]), depth);
        var file = new BmpFile(image, isTopDown: true);
        file.Image.GetPixel(0, 0).Should().Equal(new Colour(1, 2, 3));
        file.BitsPerPixel.Should().Equal(depth);
        file.IsTopDown.Should().BeTrue();
    }

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new BmpFile(null!)).Should().Throw<ArgumentNullException>();

    [TestCase(1)]
    [TestCase(2)]
    public void Constructor_UnsupportedDepth(int depth) =>
        AssertThat.Invoking(() => new BmpFile(new IndexedImageData(1, 1, [0], new PaletteData([new Colour(1, 2, 3)]), depth)))
            .Should().Throw<NotSupportedException>();

    [TestCase(false)]
    [TestCase(true)]
    public void Constructor_Alpha(bool indexed)
    {
        ImageData image = indexed
            ? new IndexedImageData(1, 1, [0], new PaletteData([new Colour(1, 2, 3, 128)]))
            : new ColourImageData(1, 1, [new Colour(1, 2, 3, 0)]);
        AssertThat.Invoking(() => new BmpFile(image)).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("image");
    }
}