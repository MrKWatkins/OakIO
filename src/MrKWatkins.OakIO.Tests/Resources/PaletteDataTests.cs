using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class PaletteDataTests
{
    [Test]
    public void Constructor()
    {
        Colour[] colours = [new(1, 2, 3), new(4, 5, 6, 0), new(1, 2, 3)];
        var palette = new PaletteData(colours);
        palette.Colours.Should().SequenceEqual(colours);
        colours[0] = new Colour(255, 255, 255);
        palette.Colours[0].Should().Equal(new Colour(1, 2, 3));
    }

    [Test]
    public void Constructor_Singleton() => new PaletteData([new Colour(1, 2, 3)]).Colours.Should().SequenceEqual(new Colour(1, 2, 3));

    [Test]
    public void Constructor_LargePalette() => new PaletteData(Enumerable.Repeat(new Colour(1, 2, 3), 257)).Colours.Should().HaveCount(257);

    [Test]
    public void Constructor_Empty() => AssertThat.Invoking(() => new PaletteData([])).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("colours");

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new PaletteData(null!)).Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("colours");
}