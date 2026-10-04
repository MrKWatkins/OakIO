using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxb;

public sealed class NxbFileTests
{
    [TestCaseSource(typeof(NxbTestData), nameof(NxbTestData.Collections))]
    public void Constructor_ByteArray(int bits, int count)
    {
        var format = new NxbFormat(3, 2, bits);
        var bytes = NxbTestData.Bytes(bits, count);
        var expected = bytes.ToArray();
        var file = new NxbFile(bytes, format);
        file.Format.Should().BeTheSameInstanceAs(format);
        ((TileBlocksFile)file).Format.Should().BeTheSameInstanceAs(format);
        file.Entries.Count.Should().Equal(count);
        file.ToByteArray().Should().SequenceEqual(expected);
        file.Blocks.Entries.Should().SequenceEqual(NxbTestData.Entries(bits, count));
        if (bytes.Length > 0)
        {
            bytes[0] ^= 255;
            bytes[^1] ^= 255;
            file.ToByteArray().Should().SequenceEqual(expected);
        }
    }

    [TestCaseSource(typeof(NxbTestData), nameof(NxbTestData.Collections))]
    public void Constructor_TileBlocksData(int bits, int count)
    {
        var blocks = new TileBlocksData(3, 2, NxbTestData.Entries(bits, count));
        var file = new NxbFile(blocks, new NxbFormat(3, 2, bits));
        file.ToByteArray().Should().SequenceEqual(NxbTestData.Bytes(bits, count));
        file.Blocks.Width.Should().Equal(3);
        file.Blocks.Height.Should().Equal(2);
        file.Blocks.Count.Should().Equal(count);
        file.Blocks.Entries.Should().SequenceEqual(blocks.Entries);
    }

    [TestCase(8)]
    [TestCase(16)]
    public void Blocks(int bits)
    {
        var file = new NxbFormat(3, 2, bits).Read(NxbTestData.Bytes(bits, 2));
        file.Entries[0].TileMap.Entries.Should().SequenceEqual(NxbTestData.Entries(bits, 1));
        file.Entries[1].TileMap.Entries.Should().SequenceEqual(NxbTestData.Entries(bits, 1).Reverse());
        file.Blocks.GetBlock(1).Entries.Should().SequenceEqual(file.Entries[1].TileMap.Entries);
        new NxmFile(file.Blocks.GetBlock(1), new NxmFormat(3, 2, bits == 8 ? NxmEncoding.Index8 : NxmEncoding.Index16))
            .ToByteArray().Should().SequenceEqual(NxbTestData.Bytes(bits, 2).Skip(6 * bits / 8));
    }

    [TestCase(8)]
    [TestCase(16)]
    public void Constructor_TileBlocksData_SingleEntry(int bits)
    {
        var blocks = new TileBlocksData(1, 1, [new(255)]);
        var file = new NxbFile(blocks, new NxbFormat(1, 1, bits));
        file.ToByteArray().Should().SequenceEqual(bits == 8 ? new byte[] { 255 } : new byte[] { 255, 0 });
        file.Blocks.Count.Should().Equal(1);
    }

    [TestCase(8)]
    [TestCase(16)]
    public void Constructor_ByteArray_LargeCollection(int bits)
    {
        var bytes = NxbTestData.Bytes(bits, 257);
        var file = new NxbFormat(3, 2, bits).Read(bytes);
        file.Entries.Count.Should().Equal(257);
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase(8, 1)]
    [TestCase(8, 5)]
    [TestCase(8, 7)]
    [TestCase(16, 1)]
    [TestCase(16, 11)]
    [TestCase(16, 13)]
    public void Constructor_ByteArray_InvalidLength(int bits, int length) =>
        AssertThat.Invoking(() => new NxbFile(new byte[length], new NxbFormat(3, 2, bits))).Should().Throw<InvalidDataException>();

    [Test]
    public void Constructor_ByteArray_Null() => AssertThat.Invoking(() => new NxbFile(bytes: null!, new NxbFormat(1, 1, 8))).Should().Throw<ArgumentNullException>();
    [Test]
    public void Constructor_TileBlocksData_Null() => AssertThat.Invoking(() => new NxbFile(blocks: null!, new NxbFormat(1, 1, 8))).Should().Throw<ArgumentNullException>();
    [Test]
    public void Constructor_NullFormat() => AssertThat.Invoking(() => new NxbFile(new TileBlocksData(1, 1, []), null!)).Should().Throw<ArgumentNullException>();

    [TestCase(2, 3)]
    [TestCase(3, 3)]
    [TestCase(4, 2)]
    public void Constructor_TileBlocksData_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new NxbFile(new TileBlocksData(width, height, []), new NxbFormat(3, 2, 8))).Should().Throw<ArgumentException>();

    [TestCase(8, 256)]
    [TestCase(8, 257)]
    [TestCase(16, 65536)]
    [TestCase(16, 65537)]
    public void Constructor_TileBlocksData_IndexOverflow(int bits, int index) =>
        AssertThat.Invoking(() => new NxbFile(new TileBlocksData(1, 1, [new(index)]), new NxbFormat(1, 1, bits))).Should().Throw<ArgumentException>();

    [TestCase(8)]
    [TestCase(16)]
    public void Constructor_TileBlocksData_Attributes(int bits)
    {
        TileMapEntry[] entries = [new(0, 1), new(0) { MirrorX = true }, new(0) { MirrorY = true }, new(0) { Rotate = true }, new(0) { Priority = true }];
        foreach (var entry in entries)
        {
            AssertThat.Invoking(() => new NxbFile(new TileBlocksData(1, 1, [entry]), new NxbFormat(1, 1, bits))).Should().Throw<ArgumentException>();
        }
    }
}