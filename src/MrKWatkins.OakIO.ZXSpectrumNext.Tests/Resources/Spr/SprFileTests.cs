using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Spr;

public sealed class SprFileTests
{
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor_TilesData(int bits)
    {
        var bytes = TileTestData.Bytes(16, bits, 2);
        var pixels = TileTestData.Pixels(bytes, bits);
        var file = new SprFile(new TilesData(16, 16, pixels, bits));
        file.Format.Should().BeTheSameInstanceAs(SprFormat.ForBitsPerPixel(bits));
        file.Patterns.Count.Should().Equal(2);
        file.Tiles.Width.Should().Equal(16);
        file.Tiles.Height.Should().Equal(16);
        file.Tiles.BitsPerPixel.Should().Equal(bits);
        file.Tiles.Pixels.Should().SequenceEqual(pixels);
        file.ToByteArray().Should().SequenceEqual(bytes);
        pixels[0] = 0;
        file.ToByteArray().Should().SequenceEqual(bytes);
    }
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor_ByteArray(int bits)
    {
        var bytes = TileTestData.Bytes(16, bits, 2);
        var expected = bytes.ToArray();
        var file = new SprFile(bytes, SprFormat.ForBitsPerPixel(bits));
        file.Patterns.Count.Should().Equal(2);
        file.Patterns[1].Data.Should().SequenceEqual(expected.Skip(expected.Length / 2));
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(expected, bits));
        bytes[0] = 0;
        file.ToByteArray().Should().SequenceEqual(expected);
    }
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(129)]
    public void Constructor_TilesData_Count(int count)
    {
        var pixels = Enumerable.Repeat((byte)15, count * 256).ToArray();
        var file = new SprFile(new TilesData(16, 16, pixels, 4));
        file.Tiles.Count.Should().Equal(count);
        file.Patterns.Count.Should().Equal(count);
        file.ToByteArray().Should().SequenceEqual(Enumerable.Repeat((byte)255, count * 128));
    }
    [Test]
    public void Constructor_TilesData_Null() =>
        AssertThat.Invoking(() => new SprFile(tiles: null!)).Should().Throw<ArgumentNullException>();
    [TestCase(8, 16)]
    [TestCase(16, 8)]
    public void Constructor_TilesData_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new SprFile(new TilesData(width, height, new byte[width * height], 4))).Should().Throw<ArgumentException>();
    [Test]
    public void Constructor_TilesData_InvalidBitDepth() =>
        AssertThat.Invoking(() => new SprFile(new TilesData(16, 16, new byte[256], 2))).Should().Throw<ArgumentOutOfRangeException>();
    [TestCase(1)]
    [TestCase(127)]
    [TestCase(129)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new SprFile(new byte[length], SprFormat.FourBit)).Should().Throw<InvalidDataException>();
}