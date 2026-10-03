using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TilesFileTests
{
    [Test]
    public void Constructor()
    {
        var data = new TilesData(1, 1, [1]);
        var file = new TestTilesFile(data);
        file.Tiles.Should().BeTheSameInstanceAs(data);
        file.Format.Should().BeTheSameInstanceAs(TestTilesFormat.Instance);
        ((ResourceFile)file).Format.Should().BeTheSameInstanceAs(TestTilesFormat.Instance);
        ((IOFile)file).Format.Should().BeTheSameInstanceAs(TestTilesFormat.Instance);
    }

    [Test]
    public void Constructor_NullFormat() =>
        AssertThat.Invoking(() => new TestTilesFile(null!, new TilesData(1, 1, [0])))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("format");

    [Test]
    public void Constructor_NullData() =>
        AssertThat.Invoking(() => new TestTilesFile(TestTilesFormat.Instance, null!))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("tiles");
}