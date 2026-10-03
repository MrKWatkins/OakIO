using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Gpl;

namespace MrKWatkins.OakIO.Tests.Resources.Gpl;

public sealed class GplFileTests
{
    [Test]
    public void Constructor()
    {
        var palette = new PaletteData([new Colour(1, 2, 3), new Colour(1, 2, 3)]);
        var file = new GplFile(palette, "名", 255, ["First", "Second"]);
        file.Header.Name.Should().Equal("名");
        file.Header.Columns.Should().Equal(255);
        file.Entries.Select(entry => entry.Name).Should().SequenceEqual("First", "Second");
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(1, 2, 3));
        file.Format.Should().BeTheSameInstanceAs(GplFormat.Instance);
        file.ToByteArray().Should().SequenceEqual(Encoding.UTF8.GetBytes("GIMP Palette\nName: 名\nColumns: 255\n#\n1 2 3\tFirst\n1 2 3\tSecond\n"));
    }

    [Test]
    public void Constructor_Defaults()
    {
        var file = new GplFile(new PaletteData([new Colour(0, 255, 128)]));
        file.Header.Name.Should().Equal("OakIO");
        file.Header.Columns.Should().Equal(0);
        file.Entries[0].Name.Should().Equal("");
        file.ToByteArray().Should().SequenceEqual(Encoding.UTF8.GetBytes("GIMP Palette\nName: OakIO\nColumns: 0\n#\n0 255 128\n"));
    }

    [TestCase(-1)]
    [TestCase(256)]
    public void Constructor_InvalidColumns(int columns) => AssertThat.Invoking(() =>
        new GplFile(new PaletteData([new Colour(1, 2, 3)]), columns: columns)).Should().Throw<ArgumentOutOfRangeException>();

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new GplFile(palette: null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_Alpha() => AssertThat.Invoking(() => new GplFile(new PaletteData([new Colour(1, 2, 3, 128)])))
        .Should().Throw<ArgumentException>();

    [TestCase("one\ntwo")]
    [TestCase("one\rtwo")]
    [TestCase(null)]
    public void Constructor_InvalidName(string? name) => AssertThat.Invoking(() =>
        new GplFile(new PaletteData([new Colour(1, 2, 3)]), name!)).Should().Throw<ArgumentException>();

    [Test]
    public void Constructor_InvalidColourNames()
    {
        var palette = new PaletteData([new Colour(1, 2, 3)]);
        AssertThat.Invoking(() => new GplFile(palette, colourNames: [])).Should().Throw<ArgumentException>();
        AssertThat.Invoking(() => new GplFile(palette, colourNames: ["First", "Second"])).Should().Throw<ArgumentException>();
        AssertThat.Invoking(() => new GplFile(palette, colourNames: ["bad\nname"])).Should().Throw<ArgumentException>();
        AssertThat.Invoking(() => new GplFile(palette, colourNames: [null!])).Should().Throw<ArgumentNullException>();
    }
}