using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources.Scr;

public sealed class ScrAttributeTests
{
    [Test]
    public void Constructor()
    {
        for (var value = 0; value < 256; value++)
        {
            var attribute = new ScrAttribute((byte)value);
            attribute.Value.Should().Equal((byte)value);
            attribute.Ink.Should().Equal((byte)(value % 8));
            attribute.Paper.Should().Equal((byte)(value / 8 % 8));
            attribute.Bright.Should().Equal(value / 64 % 2 == 1);
            attribute.Flash.Should().Equal(value >= 128);
            attribute.InkColour.Should().Equal(ScrTestData.Colours[value % 8 + value / 64 % 2 * 8]);
            attribute.PaperColour.Should().Equal(ScrTestData.Colours[value / 8 % 8 + value / 64 % 2 * 8]);
        }
    }
}