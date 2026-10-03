using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Tests.Resources.Bmp;

public sealed class BmpPaletteTests
{
    [Test]
    public void Constructor()
    {
        var bytes = BmpFormatTests.Create(4, false);
        bytes[57] = 123;
        var palette = BmpFormat.Instance.Read(bytes).Palette!;
        palette.Length.Should().Equal(24);
        palette.Count.Should().Equal(6);
        palette.Data.Should().SequenceEqual(bytes[54..78]);
        palette.Colours.Should().SequenceEqual(Enumerable.Range(0, 6).Select(i => new Colour((byte)i, 10, 20)));
    }

    [Test]
    public void Colours_DuplicateEntries()
    {
        var bytes = BmpFormatTests.Create(8, true);
        bytes[60] = bytes[56];
        var palette = BmpFormat.Instance.Read(bytes).Palette!;
        palette.Colours[1].Should().Equal(palette.Colours[0]);
        palette.Count.Should().Equal(6);
    }
}