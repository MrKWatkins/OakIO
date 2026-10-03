using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources;

public sealed class PackedPixelsTests
{
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Pack(int bits)
    {
        var bytes = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();
        PackedPixels.Pack(TileTestData.Pixels(bytes, bits), bits).Should().SequenceEqual(bytes);
    }

    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Unpack(int bits)
    {
        var bytes = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();
        PackedPixels.Unpack(bytes, bits).Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
    }

    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Pack_Empty(int bits) => PackedPixels.Pack([], bits).Should().BeEmpty();

    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Unpack_Empty(int bits) => PackedPixels.Unpack([], bits).Should().BeEmpty();

    [Test]
    public void Pack_Null() => AssertThat.Invoking(() => PackedPixels.Pack(null!, 4)).Should().Throw<ArgumentNullException>();

    [TestCase(0)]
    [TestCase(2)]
    [TestCase(16)]
    public void ValidateBitsPerPixel_Invalid(int bits)
    {
        AssertThat.Invoking(() => PackedPixels.Pack([], bits)).Should().Throw<ArgumentOutOfRangeException>();
        AssertThat.Invoking(() => PackedPixels.Unpack([], bits)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase(1)]
    [TestCase(4)]
    public void Pack_PartialByte(int bits) => AssertThat.Invoking(() => PackedPixels.Pack(new byte[1], bits)).Should().Throw<ArgumentException>();

    [TestCase(1, 2)]
    [TestCase(4, 16)]
    [TestCase(4, 255)]
    public void Pack_InvalidIndex(int bits, byte value) =>
        AssertThat.Invoking(() => PackedPixels.Pack([.. Enumerable.Repeat(value, 8)], bits)).Should().Throw<ArgumentException>();

    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void FromImage(int bits)
    {
        var source = TileTestData.Image(8, bits, out var expected);
        var result = PackedPixels.FromImage(source, 8, bits);
        result.Width.Should().Equal(8);
        result.Height.Should().Equal(8);
        result.Count.Should().Equal(4);
        result.BitsPerPixel.Should().Equal(bits);
        result.Pixels.Should().SequenceEqual(expected);
    }

    [Test]
    public void FromImage_Null() => AssertThat.Invoking(() => PackedPixels.FromImage(null!, 8, 4)).Should().Throw<ArgumentNullException>();

    [Test]
    public void FromImage_NonIndexed()
    {
        var source = new BmpFile(new ColourImageData(8, 8, [.. Enumerable.Repeat(new Colour(1, 2, 3), 64)]));
        AssertThat.Invoking(() => PackedPixels.FromImage(source, 8, 4)).Should().Throw<NotSupportedException>();
    }

    [TestCase(7, 8)]
    [TestCase(8, 7)]
    public void FromImage_InvalidDimensions(int width, int height)
    {
        var source = new BmpFile(new IndexedImageData(width, height, new byte[width * height], new PaletteData([new Colour(0, 0, 0)])));
        AssertThat.Invoking(() => PackedPixels.FromImage(source, 8, 4)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void FromImage_InvalidIndex()
    {
        var source = new BmpFile(new IndexedImageData(8, 8, [.. Enumerable.Repeat((byte)16, 64)],
            new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0), 17))));
        AssertThat.Invoking(() => PackedPixels.FromImage(source, 8, 4)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void FromTiles()
    {
        var source = new TilesData(8, 8, [.. Enumerable.Repeat((byte)15, 128)]);
        var result = PackedPixels.FromTiles(source, 8, 4);
        result.Width.Should().Equal(8);
        result.Height.Should().Equal(8);
        result.Count.Should().Equal(2);
        result.BitsPerPixel.Should().Equal(4);
        result.Pixels.Should().SequenceEqual(Enumerable.Repeat((byte)15, 128));
    }

    [Test]
    public void FromTiles_Null() => AssertThat.Invoking(() => PackedPixels.FromTiles(null!, 8, 4)).Should().Throw<ArgumentNullException>();

    [TestCase(4, 8)]
    [TestCase(8, 4)]
    public void FromTiles_InvalidDimensions(int width, int height)
    {
        var source = new TilesData(width, height, new byte[width * height]);
        AssertThat.Invoking(() => PackedPixels.FromTiles(source, 8, 4)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void FromTiles_InvalidIndex()
    {
        var source = new TilesData(8, 8, [.. Enumerable.Repeat((byte)16, 64)]);
        AssertThat.Invoking(() => PackedPixels.FromTiles(source, 8, 4)).Should().Throw<ArgumentException>();
    }

    [TestCase(0, 1)]
    [TestCase(2, 1)]
    public void ValidateLength_Invalid(int length, int expected) =>
        AssertThat.Invoking(() => PackedPixels.ValidateLength(length, expected)).Should().Throw<InvalidDataException>();
}