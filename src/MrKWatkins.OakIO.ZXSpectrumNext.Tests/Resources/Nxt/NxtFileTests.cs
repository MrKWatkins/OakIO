using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxt;

public sealed class NxtFileTests
{
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor_TilesData(int bits)
    {
        var bytes = TileTestData.Bytes(8, bits, 2);
        var pixels = TileTestData.Pixels(bytes, bits);
        var file = new NxtFile(new TilesData(8, 8, pixels, bits));
        file.Format.Should().BeTheSameInstanceAs(NxtFormat.ForBitsPerPixel(bits));
        file.Entries.Count.Should().Equal(2);
        file.Tiles.Width.Should().Equal(8);
        file.Tiles.Height.Should().Equal(8);
        file.Tiles.BitsPerPixel.Should().Equal(bits);
        file.Tiles.Pixels.Should().SequenceEqual(pixels);
        file.ToByteArray().Should().SequenceEqual(bytes);
        pixels[0] = 0;
        file.ToByteArray().Should().SequenceEqual(bytes);
    }
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor_ByteArray(int bits)
    {
        var bytes = TileTestData.Bytes(8, bits, 2);
        var expected = bytes.ToArray();
        var file = new NxtFile(bytes, NxtFormat.ForBitsPerPixel(bits));
        file.Entries.Count.Should().Equal(2);
        file.Entries[1].Data.Should().SequenceEqual(expected.Skip(expected.Length / 2));
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(expected, bits));
        bytes[0] = 0;
        file.ToByteArray().Should().SequenceEqual(expected);
    }
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(129)]
    public void Constructor_TilesData_Count(int count)
    {
        var pixels = Enumerable.Repeat((byte)15, count * 64).ToArray();
        var file = new NxtFile(new TilesData(8, 8, pixels, 4));
        file.Tiles.Count.Should().Equal(count);
        file.Entries.Count.Should().Equal(count);
        file.ToByteArray().Should().SequenceEqual(Enumerable.Repeat((byte)255, count * 32));
    }
    [Test]
    public void Constructor_TilesData_Null() =>
        AssertThat.Invoking(() => new NxtFile(tiles: null!)).Should().Throw<ArgumentNullException>();
    [TestCase(4, 8)]
    [TestCase(8, 4)]
    public void Constructor_TilesData_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new NxtFile(new TilesData(width, height, new byte[width * height], 4))).Should().Throw<ArgumentException>();
    [Test]
    public void Constructor_TilesData_InvalidBitDepth() =>
        AssertThat.Invoking(() => new NxtFile(new TilesData(8, 8, new byte[64], 2))).Should().Throw<ArgumentOutOfRangeException>();
    [TestCase(1)]
    [TestCase(31)]
    [TestCase(33)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new NxtFile(new byte[length], NxtFormat.FourBit)).Should().Throw<InvalidDataException>();
}