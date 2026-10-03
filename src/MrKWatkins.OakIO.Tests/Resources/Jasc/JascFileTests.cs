using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Jasc;

namespace MrKWatkins.OakIO.Tests.Resources.Jasc;

public sealed class JascFileTests
{
    [Test]
    public void Constructor()
    {
        var file = new JascFile(new PaletteData([new Colour(1, 2, 3), new Colour(1, 2, 3)]));
        file.Format.Should().BeTheSameInstanceAs(JascFormat.Instance);
        file.Header.ColourCount.Should().Equal(2);
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(1, 2, 3));
        file.ToByteArray().Should().SequenceEqual(Encoding.UTF8.GetBytes("JASC-PAL\n0100\n2\n1 2 3\n1 2 3\n"));
    }

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new JascFile(palette: null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_Alpha() => AssertThat.Invoking(() => new JascFile(new PaletteData([new Colour(1, 2, 3, 0)])))
        .Should().Throw<ArgumentException>();
}