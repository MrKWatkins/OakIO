using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Spr;

public sealed class TilesToSprConverterTests
{
    [Test]
    public void Constructor()
    {
        var converter = new TilesToSprConverter(SprFormat.EightBit, SprFormat.FourBit);
        converter.SourceFormat.Should().BeTheSameInstanceAs(SprFormat.EightBit);
        converter.TargetFormat.Should().BeTheSameInstanceAs(SprFormat.FourBit);
    }
    [Test]
    public void Constructor_NullSource() =>
        AssertThat.Invoking(() => new TilesToSprConverter(null!, SprFormat.FourBit)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Constructor_NullTarget() =>
        AssertThat.Invoking(() => new TilesToSprConverter(SprFormat.EightBit, null!)).Should().Throw<ArgumentNullException>();
    [TestCase(4)]
    [TestCase(8)]
    public void Convert(int bits)
    {
        var expected = TileTestData.Pixels(TileTestData.Bytes(16, bits, 2), bits);
        var source = new SprFile(new TilesData(16, 16, expected));
        var converter = new TilesToSprConverter(source.Format, SprFormat.ForBitsPerPixel(bits));
        var file = converter.Convert(source);
        file.Format.Should().BeTheSameInstanceAs(SprFormat.ForBitsPerPixel(bits));
        file.Tiles.Count.Should().Equal(2);
        file.Tiles.BitsPerPixel.Should().Equal(bits);
        file.Tiles.Pixels.Should().SequenceEqual(expected);
    }
    [Test]
    public void Convert_Null() =>
        AssertThat.Invoking(() => new TilesToSprConverter(SprFormat.EightBit, SprFormat.FourBit).Convert(null!)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Convert_InvalidIndex()
    {
        var source = new SprFile(new TilesData(16, 16, [.. Enumerable.Repeat((byte)16, 256)]));
        AssertThat.Invoking(() => new TilesToSprConverter(source.Format, SprFormat.FourBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_InvalidDimensions()
    {
        var source = new MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt.NxtFile(new TilesData(8, 8, new byte[64], 4));
        AssertThat.Invoking(() => new TilesToSprConverter(source.Format, SprFormat.FourBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_Widening()
    {
        var source = new SprFile(new TilesData(16, 16, [.. Enumerable.Repeat((byte)15, 256)], 4));
        var file = new TilesToSprConverter(source.Format, SprFormat.EightBit).Convert(source);
        file.Format.Should().BeTheSameInstanceAs(SprFormat.EightBit);
        file.Tiles.Pixels.Should().SequenceEqual(Enumerable.Repeat((byte)15, 256));
        file.ToByteArray().Should().SequenceEqual(Enumerable.Repeat((byte)15, 256));
    }
}