using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxb;

public sealed class NxbBlockTests
{
    [TestCase(8)]
    [TestCase(16)]
    public void Constructor_ByteArray(int bits)
    {
        var format = new NxbFormat(3, 2, bits);
        var bytes = NxbTestData.Bytes(bits, 1);
        var expected = bytes.ToArray();
        var block = new NxbBlock(bytes, format);
        block.Format.Should().BeTheSameInstanceAs(format);
        block.Length.Should().Equal(bits == 8 ? 6 : 12);
        block.Data.Should().SequenceEqual(expected);
        block.TileMap.Width.Should().Equal(3);
        block.TileMap.Height.Should().Equal(2);
        block.TileMap.Entries.Should().SequenceEqual(NxbTestData.Entries(bits, 1));
        bytes[0] ^= 255;
        block.Data.Should().SequenceEqual(expected);
    }

    [TestCase(8)]
    [TestCase(16)]
    public void Constructor_TileMapData(int bits)
    {
        var map = new TileMapData(3, 2, NxbTestData.Entries(bits, 1));
        new NxbBlock(map, new NxbFormat(3, 2, bits)).Data.Should().SequenceEqual(NxbTestData.Bytes(bits, 1));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(11)]
    [TestCase(13)]
    public void Constructor_ByteArray_InvalidLength(int count) =>
        AssertThat.Invoking(() => new NxbBlock(new byte[count], new NxbFormat(3, 2, 16))).Should().Throw<ArgumentException>();
}