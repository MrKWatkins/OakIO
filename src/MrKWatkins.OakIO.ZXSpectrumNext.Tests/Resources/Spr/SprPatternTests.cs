using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Spr;

public sealed class SprPatternTests
{
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor(int bits)
    {
        var bytes = TileTestData.Bytes(16, bits, 1);
        var entry = new SprPattern(bytes, bits);
        entry.Data.Should().SequenceEqual(bytes);
        entry.Length.Should().Equal(256 * bits / 8);
        entry.BitsPerPixel.Should().Equal(bits);
        entry.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
    }
    [TestCase(0)]
    [TestCase(2)]
    [TestCase(16)]
    public void Constructor_InvalidBitDepth(int bits) =>
        AssertThat.Invoking(() => new SprPattern(new byte[256], bits)).Should().Throw<ArgumentOutOfRangeException>();
    [TestCase(0)]
    [TestCase(127)]
    [TestCase(129)]
    public void Constructor_InvalidLength(int length) =>
        AssertThat.Invoking(() => new SprPattern(new byte[length], 4)).Should().Throw<InvalidDataException>();
}