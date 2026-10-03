using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Spr;

public sealed class ImageToSprConverterTests
{
    [TestCase(15, 16)]
    [TestCase(16, 15)]
    public void Convert_InvalidDimensions(int width, int height)
    {
        var source = new BmpFile(new IndexedImageData(width, height, new byte[width * height], new PaletteData([new Colour(0, 0, 0)])));
        AssertThat.Invoking(() => new ImageToSprConverter(source.Format, SprFormat.FourBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Constructor()
    {
        var converter = new ImageToSprConverter(BmpFormat.Instance, SprFormat.FourBit);
        converter.SourceFormat.Should().BeTheSameInstanceAs(BmpFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(SprFormat.FourBit);
    }
    [Test]
    public void Constructor_NullSource() =>
        AssertThat.Invoking(() => new ImageToSprConverter(null!, SprFormat.FourBit)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Constructor_NullTarget() =>
        AssertThat.Invoking(() => new ImageToSprConverter(BmpFormat.Instance, null!)).Should().Throw<ArgumentNullException>();
    [TestCase(4)]
    [TestCase(8)]
    public void Convert(int bits)
    {
        var source = TileTestData.Image(16, bits, out var expected);
        var converter = new ImageToSprConverter(source.Format, SprFormat.ForBitsPerPixel(bits));
        var file = converter.Convert(source);
        file.Format.Should().BeTheSameInstanceAs(SprFormat.ForBitsPerPixel(bits));
        file.Tiles.Count.Should().Equal(4);
        file.Tiles.BitsPerPixel.Should().Equal(bits);
        file.Tiles.Pixels.Should().SequenceEqual(expected);
    }
    [Test]
    public void Convert_Null() =>
        AssertThat.Invoking(() => new ImageToSprConverter(BmpFormat.Instance, SprFormat.FourBit).Convert(null!)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Convert_InvalidIndex()
    {
        var source = new BmpFile(new IndexedImageData(16, 16, [.. Enumerable.Repeat((byte)16, 256)], new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0), 17))));
        AssertThat.Invoking(() => new ImageToSprConverter(source.Format, SprFormat.FourBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_NonIndexed()
    {
        var source = new BmpFile(new ColourImageData(16, 16, [.. Enumerable.Repeat(new Colour(1, 2, 3), 256)]));
        AssertThat.Invoking(() => new ImageToSprConverter(source.Format, SprFormat.FourBit).Convert(source)).Should().Throw<NotSupportedException>();
    }

}