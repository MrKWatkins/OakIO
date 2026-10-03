using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxt;

public sealed class ImageToNxtConverterTests
{
    [TestCase(7, 8)]
    [TestCase(8, 7)]
    public void Convert_InvalidDimensions(int width, int height)
    {
        var source = new BmpFile(new IndexedImageData(width, height, new byte[width * height], new PaletteData([new Colour(0, 0, 0)])));
        AssertThat.Invoking(() => new ImageToNxtConverter(source.Format, NxtFormat.FourBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_InvalidIndex_OneBit()
    {
        var source = new BmpFile(new IndexedImageData(8, 8, [.. Enumerable.Repeat((byte)2, 64)], new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0), 3))));
        AssertThat.Invoking(() => new ImageToNxtConverter(source.Format, NxtFormat.OneBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Constructor()
    {
        var converter = new ImageToNxtConverter(BmpFormat.Instance, NxtFormat.FourBit);
        converter.SourceFormat.Should().BeTheSameInstanceAs(BmpFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(NxtFormat.FourBit);
    }
    [Test]
    public void Constructor_NullSource() =>
        AssertThat.Invoking(() => new ImageToNxtConverter(null!, NxtFormat.FourBit)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Constructor_NullTarget() =>
        AssertThat.Invoking(() => new ImageToNxtConverter(BmpFormat.Instance, null!)).Should().Throw<ArgumentNullException>();
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Convert(int bits)
    {
        var source = TileTestData.Image(8, bits, out var expected);
        var converter = new ImageToNxtConverter(source.Format, NxtFormat.ForBitsPerPixel(bits));
        var file = converter.Convert(source);
        file.Format.Should().BeTheSameInstanceAs(NxtFormat.ForBitsPerPixel(bits));
        file.Tiles.Count.Should().Equal(4);
        file.Tiles.BitsPerPixel.Should().Equal(bits);
        file.Tiles.Pixels.Should().SequenceEqual(expected);
    }
    [Test]
    public void Convert_Null() =>
        AssertThat.Invoking(() => new ImageToNxtConverter(BmpFormat.Instance, NxtFormat.FourBit).Convert(null!)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Convert_InvalidIndex()
    {
        var source = new BmpFile(new IndexedImageData(8, 8, [.. Enumerable.Repeat((byte)16, 64)], new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0), 17))));
        AssertThat.Invoking(() => new ImageToNxtConverter(source.Format, NxtFormat.FourBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_NonIndexed()
    {
        var source = new BmpFile(new ColourImageData(8, 8, [.. Enumerable.Repeat(new Colour(1, 2, 3), 64)]));
        AssertThat.Invoking(() => new ImageToNxtConverter(source.Format, NxtFormat.FourBit).Convert(source)).Should().Throw<NotSupportedException>();
    }

}