using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.Tests.Resources.PaintNet;

public sealed class PaintNetFileTests
{
    [Test]
    public void Constructor()
    {
        var file = new PaintNetFile(new PaletteData([new Colour(1, 2, 3, 0), new Colour(255, 128, 0, 128)]));
        file.Format.Should().BeTheSameInstanceAs(PaintNetFormat.Instance);
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3, 0), new Colour(255, 128, 0, 128));
        file.ToByteArray().Should().SequenceEqual(Encoding.UTF8.GetBytes("; Paint.NET Palette File\n00010203\n80FF8000\n"));
    }

    [TestCase(1)]
    [TestCase(95)]
    [TestCase(96)]
    [TestCase(97)]
    [TestCase(256)]
    public void Constructor_ActualEntries(int count)
    {
        var colours = Enumerable.Range(0, count).Select(i => new Colour((byte)i, 2, 3)).ToArray();
        var file = new PaintNetFile(new PaletteData(colours));
        file.Entries.Count.Should().Equal(count);
        file.Palette.Colours.Should().SequenceEqual(colours);
        PaintNetFormat.Instance.Read(file.ToByteArray()).Palette.Colours.Should().SequenceEqual(colours);
    }

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new PaintNetFile(palette: null!)).Should().Throw<ArgumentNullException>();
}