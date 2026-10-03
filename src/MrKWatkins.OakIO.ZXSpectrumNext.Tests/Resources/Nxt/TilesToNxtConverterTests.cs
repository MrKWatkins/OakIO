using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxt;

public sealed class TilesToNxtConverterTests
{
    [Test]
    public void Convert_InvalidIndex_OneBit()
    {
        var source = new NxtFile(new TilesData(8, 8, [.. Enumerable.Repeat((byte)2, 64)]));
        AssertThat.Invoking(() => new TilesToNxtConverter(source.Format, NxtFormat.OneBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Constructor()
    {
        var converter = new TilesToNxtConverter(NxtFormat.EightBit, NxtFormat.FourBit);
        converter.SourceFormat.Should().BeTheSameInstanceAs(NxtFormat.EightBit);
        converter.TargetFormat.Should().BeTheSameInstanceAs(NxtFormat.FourBit);
    }
    [Test]
    public void Constructor_NullSource() =>
        AssertThat.Invoking(() => new TilesToNxtConverter(null!, NxtFormat.FourBit)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Constructor_NullTarget() =>
        AssertThat.Invoking(() => new TilesToNxtConverter(NxtFormat.EightBit, null!)).Should().Throw<ArgumentNullException>();
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Convert(int bits)
    {
        var expected = TileTestData.Pixels(TileTestData.Bytes(8, bits, 2), bits);
        var source = new NxtFile(new TilesData(8, 8, expected));
        var converter = new TilesToNxtConverter(source.Format, NxtFormat.ForBitsPerPixel(bits));
        var file = converter.Convert(source);
        file.Format.Should().BeTheSameInstanceAs(NxtFormat.ForBitsPerPixel(bits));
        file.Tiles.Count.Should().Equal(2);
        file.Tiles.BitsPerPixel.Should().Equal(bits);
        file.Tiles.Pixels.Should().SequenceEqual(expected);
    }
    [Test]
    public void Convert_Null() =>
        AssertThat.Invoking(() => new TilesToNxtConverter(NxtFormat.EightBit, NxtFormat.FourBit).Convert(null!)).Should().Throw<ArgumentNullException>();
    [Test]
    public void Convert_InvalidIndex()
    {
        var source = new NxtFile(new TilesData(8, 8, [.. Enumerable.Repeat((byte)16, 64)]));
        AssertThat.Invoking(() => new TilesToNxtConverter(source.Format, NxtFormat.FourBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_InvalidDimensions()
    {
        var source = new MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr.SprFile(new TilesData(16, 16, new byte[256], 4));
        AssertThat.Invoking(() => new TilesToNxtConverter(source.Format, NxtFormat.FourBit).Convert(source)).Should().Throw<ArgumentException>();
    }
    [Test]
    public void Convert_Widening()
    {
        var source = new NxtFile(new TilesData(8, 8, [.. Enumerable.Repeat((byte)15, 64)], 4));
        var file = new TilesToNxtConverter(source.Format, NxtFormat.EightBit).Convert(source);
        file.Format.Should().BeTheSameInstanceAs(NxtFormat.EightBit);
        file.Tiles.Pixels.Should().SequenceEqual(Enumerable.Repeat((byte)15, 64));
        file.ToByteArray().Should().SequenceEqual(Enumerable.Repeat((byte)15, 64));
    }
}