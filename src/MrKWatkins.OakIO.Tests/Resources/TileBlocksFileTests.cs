using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TileBlocksFileTests
{
    [Test]
    public void Constructor()
    {
        var data = new TileBlocksData(1, 1, [new TileMapEntry(1)]);
        var file = new TestTileBlocksFile(data);
        file.Blocks.Should().BeTheSameInstanceAs(data);
        file.Format.Should().BeTheSameInstanceAs(TestTileBlocksFormat.Instance);
        ((ResourceFile)file).Format.Should().BeTheSameInstanceAs(TestTileBlocksFormat.Instance);
        ((IOFile)file).Format.Should().BeTheSameInstanceAs(TestTileBlocksFormat.Instance);
    }

    [Test]
    public void Constructor_NullFormat() =>
        AssertThat.Invoking(() => new TestTileBlocksFile(null!, new TileBlocksData(1, 1, [new TileMapEntry(0)])))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("format");

    [Test]
    public void Constructor_NullData() =>
        AssertThat.Invoking(() => new TestTileBlocksFile(TestTileBlocksFormat.Instance, null!))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("blocks");
}