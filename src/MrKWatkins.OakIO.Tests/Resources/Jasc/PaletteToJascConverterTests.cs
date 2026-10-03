using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.Tests.Resources.Jasc;

public sealed class PaletteToJascConverterTests
{
    [Test]
    public void Constructor()
    {
        var converter = new PaletteToJascConverter(TestPaletteFormat.Instance);
        converter.SourceFormat.Should().BeTheSameInstanceAs(TestPaletteFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(JascFormat.Instance);
    }

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new PaletteToJascConverter(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert()
    {
        var source = new TestPaletteFile(new PaletteData([new Colour(1, 2, 3), new Colour(1, 2, 3), new Colour(255, 128, 0)]));
        var result = new PaletteToJascConverter(source.Format).Convert(source);
        result.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(1, 2, 3), new Colour(255, 128, 0));
        result.Format.Should().BeTheSameInstanceAs(JascFormat.Instance);
    }

    [TestCase(0)]
    [TestCase(1)]
    public void Convert_Registered(int kind)
    {
        var palette = new PaletteData([new Colour(1, 2, 3), new Colour(1, 2, 3), new Colour(255, 128, 0)]);
        PaletteFile source = kind == 0 ? new GplFile(palette) : new PaintNetFile(palette);
        var result = IOFileConversion.Convert<JascFile>(source);
        result.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(1, 2, 3), new Colour(255, 128, 0));
        IOFileConversion.Convert(source, JascFormat.Instance).Should().BeOfType<JascFile>()
            .Value.Palette.Colours.Should().SequenceEqual(result.Palette.Colours);
    }

    [Test]
    public void Convert_Null() => AssertThat.Invoking(() => new PaletteToJascConverter(TestPaletteFormat.Instance).Convert(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert_Alpha()
    {
        var source = new PaintNetFile(new PaletteData([new Colour(1, 2, 3, 128)]));
        AssertThat.Invoking(() => IOFileConversion.Convert<JascFile>(source)).Should().Throw<ArgumentException>();
    }
}