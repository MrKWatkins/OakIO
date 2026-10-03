using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TilesDataTests
{
    [Test]
    public void Constructor()
    {
        byte[] pixels = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
        var tiles = new TilesData(2, 3, pixels);
        tiles.Width.Should().Equal(2);
        tiles.Height.Should().Equal(3);
        tiles.Count.Should().Equal(2);
        tiles.BitsPerPixel.Should().Equal(8);
        tiles.Pixels.Should().SequenceEqual(pixels);
        pixels[0] = 255;
        tiles.Pixels[0].Should().Equal((byte)0);
        AssertThat.Invoking(() => ((IList<byte>)tiles.Pixels)[0] = 255).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Constructor_Empty()
    {
        var tiles = new TilesData(16, 16, [], 4);
        tiles.Count.Should().Equal(0);
        tiles.Pixels.Should().BeEmpty();
        tiles.BitsPerPixel.Should().Equal(4);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor_BitDepth(int bits)
    {
        var tiles = new TilesData(2, 1, [0, (byte)((1 << bits) - 1)], bits);
        tiles.BitsPerPixel.Should().Equal(bits);
        tiles.Count.Should().Equal(1);
        tiles.Pixels.Should().SequenceEqual(0, (byte)((1 << bits) - 1));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    public void Constructor_IndexTooLarge(int bits) =>
        AssertThat.Invoking(() => new TilesData(2, 1, [0, (byte)(1 << bits)], bits)).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("pixels");

    [TestCase(1)]
    [TestCase(3)]
    [TestCase(5)]
    public void Constructor_PartialTile(int count) =>
        AssertThat.Invoking(() => new TilesData(2, 2, new byte[count])).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("pixels");

    [TestCase(0, 1, "width")]
    [TestCase(-1, 1, "width")]
    [TestCase(1, 0, "height")]
    [TestCase(1, -1, "height")]
    public void Constructor_InvalidDimensions(int width, int height, string parameter) =>
        AssertThat.Invoking(() => new TilesData(width, height, [])).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal(parameter);

    [Test]
    public void Constructor_Overflow() => AssertThat.Invoking(() => new TilesData(int.MaxValue, 2, [])).Should().Throw<OverflowException>();

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(9)]
    public void Constructor_InvalidBitDepth(int bits) =>
        AssertThat.Invoking(() => new TilesData(1, 1, [0], bits)).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal("bitsPerPixel");

    [Test]
    public void Constructor_NullPixels() =>
        AssertThat.Invoking(() => new TilesData(1, 1, null!)).Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("pixels");
}