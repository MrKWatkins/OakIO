using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TileMapDataTests
{
    [TestCase(1, 1)]
    [TestCase(3, 2)]
    public void Constructor(int width, int height)
    {
        var entries = Enumerable.Range(0, width * height).Select(index => new TileMapEntry(index, index)).ToArray();
        var expected = entries.ToArray();
        var map = new TileMapData(width, height, entries);
        map.Width.Should().Equal(width);
        map.Height.Should().Equal(height);
        map.Entries.Should().SequenceEqual(expected);
        entries[0] = new TileMapEntry(255);
        map.Entries.Should().SequenceEqual(expected);
    }

    [TestCase(0, 1)]
    [TestCase(-1, 1)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
    public void Constructor_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new TileMapData(width, height, [])).Should().Throw<ArgumentOutOfRangeException>();

    [Test]
    public void Constructor_Overflow() => AssertThat.Invoking(() => new TileMapData(int.MaxValue, 2, [])).Should().Throw<OverflowException>();

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new TileMapData(1, 1, null!)).Should().Throw<ArgumentNullException>();

    [TestCase(0)]
    [TestCase(5)]
    [TestCase(7)]
    public void Constructor_InvalidEntryCount(int count) =>
        AssertThat.Invoking(() => new TileMapData(3, 2, new TileMapEntry[count])).Should().Throw<ArgumentException>();

    [TestCase(0, 0, 0)]
    [TestCase(2, 0, 2)]
    [TestCase(1, 1, 4)]
    [TestCase(2, 1, 5)]
    public void GetEntry(int x, int y, int expected)
    {
        var map = new TileMapData(3, 2, Enumerable.Range(0, 6).Select(index => new TileMapEntry(index)).ToArray());
        map.GetEntry(x, y).TileIndex.Should().Equal(expected);
    }

    [TestCase(-1, 0)]
    [TestCase(3, 0)]
    [TestCase(0, -1)]
    [TestCase(0, 2)]
    public void GetEntry_Invalid(int x, int y) =>
        AssertThat.Invoking(() => new TileMapData(3, 2, new TileMapEntry[6]).GetEntry(x, y)).Should().Throw<ArgumentOutOfRangeException>();
}