using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class PaletteFileTests
{
    [Test]
    public void Constructor()
    {
        var data = new PaletteData([new Colour(1, 2, 3)]);
        var file = new TestPaletteFile(data);
        file.Palette.Should().BeTheSameInstanceAs(data);
        file.Format.Should().BeTheSameInstanceAs(TestPaletteFormat.Instance);
        ((ResourceFile)file).Format.Should().BeTheSameInstanceAs(TestPaletteFormat.Instance);
        ((IOFile)file).Format.Should().BeTheSameInstanceAs(TestPaletteFormat.Instance);
    }

    [Test]
    public void Constructor_NullFormat() =>
        AssertThat.Invoking(() => new TestPaletteFile(null!, new PaletteData([default])))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("format");

    [Test]
    public void Constructor_NullData() =>
        AssertThat.Invoking(() => new TestPaletteFile(TestPaletteFormat.Instance, null!))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("palette");
}