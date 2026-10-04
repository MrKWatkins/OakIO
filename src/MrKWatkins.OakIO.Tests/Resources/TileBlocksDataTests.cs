using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TileBlocksDataTests
{
    [TestCase(1, 1, 0)]
    [TestCase(1, 1, 1)]
    [TestCase(3, 2, 0)]
    [TestCase(3, 2, 1)]
    [TestCase(3, 2, 3)]
    public void Constructor(int width, int height, int count)
    {
        var entries = Enumerable.Range(0, width * height * count).Select(index => new TileMapEntry(index, index) { MirrorX = true, MirrorY = true, Rotate = true, Priority = true }).ToArray();
        var expected = entries.ToArray();
        var blocks = new TileBlocksData(width, height, entries);
        blocks.Width.Should().Equal(width);
        blocks.Height.Should().Equal(height);
        blocks.Count.Should().Equal(count);
        blocks.Entries.Should().SequenceEqual(expected);
        if (entries.Length > 0)
        {
            entries[0] = new TileMapEntry(65535);
            blocks.Entries.Should().SequenceEqual(expected);
        }
    }

    [TestCase(0, 1)]
    [TestCase(-1, 1)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
    public void Constructor_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new TileBlocksData(width, height, [])).Should().Throw<ArgumentOutOfRangeException>();
    [Test]
    public void Constructor_Overflow() => AssertThat.Invoking(() => new TileBlocksData(int.MaxValue, 2, [])).Should().Throw<OverflowException>();
    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new TileBlocksData(1, 1, null!)).Should().Throw<ArgumentNullException>();
    [TestCase(1)]
    [TestCase(5)]
    [TestCase(7)]
    [TestCase(13)]
    public void Constructor_InvalidEntryCount(int count) =>
        AssertThat.Invoking(() => new TileBlocksData(3, 2, new TileMapEntry[count])).Should().Throw<ArgumentException>();

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void GetBlock(int index)
    {
        var entries = Enumerable.Range(0, 18).Select(value => new TileMapEntry(value, value) { MirrorX = true }).ToArray();
        var blocks = new TileBlocksData(3, 2, entries);
        var block = blocks.GetBlock(index);
        block.Width.Should().Equal(3);
        block.Height.Should().Equal(2);
        block.Entries.Should().SequenceEqual(entries.Skip(index * 6).Take(6));
        block.GetEntry(0, 0).TileIndex.Should().Equal(index * 6);
        block.GetEntry(2, 1).TileIndex.Should().Equal(index * 6 + 5);
        block.Should().NotBeTheSameInstanceAs(blocks.GetBlock(index));
        blocks.Entries.Should().SequenceEqual(entries);
    }

    [TestCase(0, -1)]
    [TestCase(0, 0)]
    [TestCase(1, -1)]
    [TestCase(1, 1)]
    [TestCase(1, 2)]
    public void GetBlock_Invalid(int count, int index) =>
        AssertThat.Invoking(() => new TileBlocksData(1, 1, new TileMapEntry[count]).GetBlock(index)).Should().Throw<ArgumentOutOfRangeException>();
}