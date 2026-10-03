using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Tests.Resources.Text;

public sealed class PaletteTextTests
{
    [Test]
    public void ReadLines()
    {
        var bytes = Encoding.UTF8.GetBytes("\uFEFFa\r\nb\nc\rd");
        var lines = PaletteText.ReadLines(bytes);
        lines.Select(line => line.Text).Should().SequenceEqual("a", "b", "c", "d");
        lines.Select(line => line.LineEnding).Should().SequenceEqual("\r\n", "\n", "\r", "");
        PaletteText.Join(lines).Should().SequenceEqual(bytes);
    }

    [Test]
    public void ReadLines_Empty() => PaletteText.ReadLines([]).Count.Should().Equal(0);

    [Test]
    public void Decode_InvalidUtf8() => AssertThat.Invoking(() => PaletteText.Decode([0xC3, 0x28]))
        .Should().Throw<InvalidDataException>().That.InnerException.Should().BeOfType<DecoderFallbackException>();

    [TestCase("0 255 128", false, "")]
    [TestCase("  0\t255   128  名称  ", true, "名称")]
    [TestCase("0 255 128", true, "")]
    public void ParseRgb(string text, bool names, string name)
    {
        var parsed = PaletteText.ParseRgb(text, names);
        parsed.Colour.Should().Equal(new Colour(0, 255, 128));
        parsed.Name.Should().Equal(name);
    }

    [TestCase("")]
    [TestCase("1")]
    [TestCase("1 2")]
    [TestCase("1 2 3 extra")]
    [TestCase("256 2 3")]
    [TestCase("1 -1 3")]
    [TestCase("1 2 x")]
    [TestCase("+1 2 3")]
    public void ParseRgb_Invalid(string text) => AssertThat.Invoking(() => PaletteText.ParseRgb(text, false)).Should().Throw<InvalidDataException>();

    [TestCase("FF010203", 255)]
    [TestCase("80010203", 128)]
    [TestCase(" 00010203 ", 0)]
    public void ParseArgb(string text, int alpha) => PaletteText.ParseArgb(text).Should().Equal(new Colour(1, 2, 3, (byte)alpha));

    [TestCase("")]
    [TestCase("FFFFFF")]
    [TestCase("FFFFFFFFF")]
    [TestCase("GG000000")]
    [TestCase("#010203")]
    public void ParseArgb_Invalid(string text) => AssertThat.Invoking(() => PaletteText.ParseArgb(text)).Should().Throw<InvalidDataException>();

    [Test]
    public void Encode() => PaletteText.Encode("名").Should().SequenceEqual(0xE5, 0x90, 0x8D);

    [Test]
    public void ValidateOpaque()
    {
        PaletteText.ValidateOpaque(new PaletteData([new Colour(1, 2, 3)]));
        AssertThat.Invoking(() => PaletteText.ValidateOpaque(null!)).Should().Throw<ArgumentNullException>();
        AssertThat.Invoking(() => PaletteText.ValidateOpaque(new PaletteData([new Colour(1, 2, 3, 0)])))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("palette");
    }

    [TestCase("\n")]
    [TestCase("\r")]
    public void ValidateSingleLine_Invalid(string text) => AssertThat.Invoking(() => PaletteText.ValidateSingleLine(text, "name"))
        .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("name");

    [Test]
    public void ValidateSingleLine_Null() => AssertThat.Invoking(() => PaletteText.ValidateSingleLine(null!, "name"))
        .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("name");
}