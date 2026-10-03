using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.Tests.Resources.PaintNet;

public sealed class PaletteToPaintNetConverterTests
{
    [Test]
    public void Constructor()
    {
        var converter = new PaletteToPaintNetConverter(TestPaletteFormat.Instance);
        converter.SourceFormat.Should().BeTheSameInstanceAs(TestPaletteFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(PaintNetFormat.Instance);
    }

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new PaletteToPaintNetConverter(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert()
    {
        var source = new TestPaletteFile(new PaletteData([new Colour(1, 2, 3), new Colour(1, 2, 3), new Colour(255, 128, 0)]));
        var result = new PaletteToPaintNetConverter(source.Format).Convert(source);
        result.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(1, 2, 3), new Colour(255, 128, 0));
        result.Format.Should().BeTheSameInstanceAs(PaintNetFormat.Instance);
    }

    [TestCase(0)]
    [TestCase(1)]
    public void Convert_Registered(int kind)
    {
        var palette = new PaletteData([new Colour(1, 2, 3), new Colour(1, 2, 3), new Colour(255, 128, 0)]);
        PaletteFile source = kind == 0 ? new GplFile(palette) : new JascFile(palette);
        var result = IOFileConversion.Convert<PaintNetFile>(source);
        result.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(1, 2, 3), new Colour(255, 128, 0));
        IOFileConversion.Convert(source, PaintNetFormat.Instance).Should().BeOfType<PaintNetFile>()
            .Value.Palette.Colours.Should().SequenceEqual(result.Palette.Colours);
    }

    [Test]
    public void Convert_Null() => AssertThat.Invoking(() => new PaletteToPaintNetConverter(TestPaletteFormat.Instance).Convert(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert_Alpha()
    {
        var source = new PaintNetFile(new PaletteData([new Colour(1, 2, 3, 128)]));
        new PaletteToPaintNetConverter(source.Format).Convert(source).Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3, 128));
    }
}