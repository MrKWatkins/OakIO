using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TileMapFileTests
{
    [Test]
    public void Constructor()
    {
        var data = new TileMapData(1, 1, [new TileMapEntry(1)]);
        var file = new TestTileMapFile(data);
        file.TileMap.Should().BeTheSameInstanceAs(data);
        file.Format.Should().BeTheSameInstanceAs(TestTileMapFormat.Instance);
        ((ResourceFile)file).Format.Should().BeTheSameInstanceAs(TestTileMapFormat.Instance);
        ((IOFile)file).Format.Should().BeTheSameInstanceAs(TestTileMapFormat.Instance);
    }

    [Test]
    public void Constructor_NullFormat() =>
        AssertThat.Invoking(() => new TestTileMapFile(null!, new TileMapData(1, 1, [new TileMapEntry(0)])))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("format");

    [Test]
    public void Constructor_NullData() =>
        AssertThat.Invoking(() => new TestTileMapFile(TestTileMapFormat.Instance, null!))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("tilemap");
}