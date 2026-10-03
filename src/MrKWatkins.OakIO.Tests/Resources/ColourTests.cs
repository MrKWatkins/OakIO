using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ColourTests
{
    [Test]
    public void Constructor()
    {
        var colour = new Colour(1, 127, 255);
        colour.Red.Should().Equal((byte)1);
        colour.Green.Should().Equal((byte)127);
        colour.Blue.Should().Equal((byte)255);
        colour.Alpha.Should().Equal((byte)255);
    }

    [TestCase(0)]
    [TestCase(128)]
    [TestCase(255)]
    public void Constructor_Alpha(byte alpha) => new Colour(1, 2, 3, alpha).Alpha.Should().Equal(alpha);

    [Test]
    public void Equals()
    {
        var colour = new Colour(1, 2, 3, 4);
        colour.Should().Equal(new Colour(1, 2, 3, 4));
        (colour == new Colour(1, 2, 3, 4)).Should().BeTrue();
        (colour == new Colour(0, 2, 3, 4)).Should().BeFalse();
        (colour == new Colour(1, 0, 3, 4)).Should().BeFalse();
        (colour == new Colour(1, 2, 0, 4)).Should().BeFalse();
        (colour == new Colour(1, 2, 3, 0)).Should().BeFalse();
    }
}