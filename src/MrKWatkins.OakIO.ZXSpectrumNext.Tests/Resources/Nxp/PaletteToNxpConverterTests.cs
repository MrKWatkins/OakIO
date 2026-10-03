using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.PaintNet;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;
using MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxp;

public sealed class PaletteToNxpConverterTests
{
    [Test]
    public void Constructor()
    {
        var converter = new PaletteToNxpConverter(PaintNetFormat.Instance);
        converter.SourceFormat.Should().BeTheSameInstanceAs(PaintNetFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(NxpFormat.Instance);
    }

    [Test]
    public void Constructor_Null() =>
        AssertThat.Invoking(() => new PaletteToNxpConverter(null!)).Should().Throw<ArgumentNullException>();

    [TestCase(16)]
    [TestCase(256)]
    public void Convert_PaletteFile(int count)
    {
        var colours = Enumerable.Repeat(new Colour(0, 255, 0), count).ToArray();
        colours[0] = new Colour(128, 36, 255);
        var source = new PaintNetFile(new PaletteData(colours));
        var file = new PaletteToNxpConverter(source.Format).Convert(source);
        file.Entries[0].Data.Should().SequenceEqual(0x87, 1);
        file.Palette.Colours.Should().SequenceEqual(new[] { new Colour(146, 36, 255) }.Concat(
            Enumerable.Repeat(new Colour(0, 255, 0), count - 1)));
        file.Format.Should().BeTheSameInstanceAs(NxpFormat.Instance);
    }

    [Test]
    public void Convert_PaletteFile_Null() =>
        AssertThat.Invoking(() => new PaletteToNxpConverter(PaintNetFormat.Instance).Convert(null!))
            .Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert_PaletteFile_InvalidCount()
    {
        var source = new PaintNetFile(new PaletteData([new Colour(0, 0, 0)]));
        AssertThat.Invoking(() => new PaletteToNxpConverter(source.Format).Convert(source)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Convert_PaletteFile_Alpha()
    {
        var source = new PaintNetFile(new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0, 128), 16)));
        AssertThat.Invoking(() => new PaletteToNxpConverter(source.Format).Convert(source)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Convert_IOFile()
    {
        IOFileConverter converter = new PaletteToNxpConverter(PaintNetFormat.Instance);
        var source = new PaintNetFile(new PaletteData(Enumerable.Repeat(new Colour(255, 255, 255), 16)));
        converter.Convert(source).Should().BeOfType<NxpFile>().Value.Entries[0].Data.Should().SequenceEqual(255, 1);
    }

    [Test]
    public void Convert_IOFile_InvalidType()
    {
        IOFileConverter converter = new PaletteToNxpConverter(PaintNetFormat.Instance);
        AssertThat.Invoking(() => converter.Convert(NexFile.CreateCode([], pc: 0x8000, sp: 0xFFFE))).Should().Throw<ArgumentException>();
    }
}