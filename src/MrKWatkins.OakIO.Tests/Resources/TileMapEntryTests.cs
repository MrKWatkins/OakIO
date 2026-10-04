using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TileMapEntryTests
{
    [TestCase(0, 0)]
    [TestCase(65536, 128)]
    [TestCase(int.MaxValue, int.MaxValue)]
    public void Constructor(int index, int palette)
    {
        var entry = new TileMapEntry(index, palette);
        entry.TileIndex.Should().Equal(index);
        entry.PaletteOffset.Should().Equal(palette);
        entry.MirrorX.Should().BeFalse();
        entry.MirrorY.Should().BeFalse();
        entry.Rotate.Should().BeFalse();
        entry.Priority.Should().BeFalse();
    }

    [TestCase(-1, 0, "tileIndex")]
    [TestCase(0, -1, "paletteOffset")]
    public void Constructor_Invalid(int index, int palette, string parameter) =>
        AssertThat.Invoking(() => new TileMapEntry(index, palette)).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal(parameter);

    [Test]
    public void Properties()
    {
        var entry = new TileMapEntry(42, 7) { MirrorX = true, MirrorY = true, Rotate = true, Priority = true };
        entry.MirrorX.Should().BeTrue();
        entry.MirrorY.Should().BeTrue();
        entry.Rotate.Should().BeTrue();
        entry.Priority.Should().BeTrue();
        entry.TileIndex.Should().Equal(42);
        entry.PaletteOffset.Should().Equal(7);
        default(TileMapEntry).Should().Equal(new TileMapEntry(0));
        entry.Should().NotEqual(entry with { Priority = false });
    }
}