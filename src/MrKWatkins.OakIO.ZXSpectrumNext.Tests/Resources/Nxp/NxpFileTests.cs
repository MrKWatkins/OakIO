using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxp;

public sealed class NxpFileTests
{
    [TestCase(16)]
    [TestCase(256)]
    public void Constructor_PaletteData(int count)
    {
        var colours = Enumerable.Repeat(new Colour(255, 128, 36), count).ToArray();
        colours[0] = new Colour(0, 0, 0);
        var file = new NxpFile(new PaletteData(colours));
        file.Format.Should().BeTheSameInstanceAs(NxpFormat.Instance);
        file.Entries.Count.Should().Equal(count);
        file.Palette.Colours.Should().SequenceEqual(
            new[] { new Colour(0, 0, 0) }.Concat(Enumerable.Repeat(new Colour(255, 146, 36), count - 1)));
        file.ToByteArray().Should().SequenceEqual(new byte[] { 0, 0 }.Concat(
            Enumerable.Range(1, count - 1).SelectMany(_ => new byte[] { 0xF0, 1 })));
    }

    [Test]
    public void Constructor_PaletteData_Copies()
    {
        var colours = Enumerable.Repeat(new Colour(255, 0, 0), 16).ToArray();
        var file = new NxpFile(new PaletteData(colours));
        colours[0] = new Colour(0, 0, 255);
        file.Entries[0].Colour.Should().Equal(new Colour(255, 0, 0));
    }

    [TestCase(1)]
    [TestCase(15)]
    [TestCase(17)]
    [TestCase(255)]
    [TestCase(257)]
    public void Constructor_PaletteData_InvalidCount(int count) =>
        AssertThat.Invoking(() => new NxpFile(new PaletteData(Enumerable.Repeat(new Colour(), count))))
            .Should().Throw<ArgumentException>();

    [Test]
    public void Constructor_PaletteData_Null() =>
        AssertThat.Invoking(() => new NxpFile(palette: null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_PaletteData_Alpha()
    {
        var colours = Enumerable.Repeat(new Colour(1, 2, 3), 16).ToArray();
        colours[^1] = new Colour(1, 2, 3, 128);
        AssertThat.Invoking(() => new NxpFile(new PaletteData(colours))).Should().Throw<ArgumentException>();
    }

    [TestCase(16)]
    [TestCase(256)]
    public void Constructor_ByteArray(int count)
    {
        var bytes = NxpFormatTests.CreateBytes(count);
        var file = new NxpFile(bytes);
        file.Entries.Count.Should().Equal(count);
        file.Entries[1].Data.Should().SequenceEqual(0x25, 1);
        file.Palette.Colours[1].Should().Equal(new Colour(36, 36, 109));
        file.ToByteArray().Should().SequenceEqual(bytes);
        bytes[2] = 0;
        file.Entries[1].Data.Should().SequenceEqual(0x25, 1);
    }

    [TestCase(0)]
    [TestCase(31)]
    [TestCase(33)]
    [TestCase(511)]
    [TestCase(513)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new NxpFile(new byte[length])).Should().Throw<InvalidDataException>();

    [Test]
    public void Constructor_ByteArray_InvalidEntry()
    {
        var bytes = new byte[32];
        bytes[^1] = 2;
        AssertThat.Invoking(() => new NxpFile(bytes)).Should().Throw<InvalidDataException>();
    }
}