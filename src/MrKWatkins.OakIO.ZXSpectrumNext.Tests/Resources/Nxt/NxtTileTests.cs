using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxt;

public sealed class NxtTileTests
{
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Constructor(int bits)
    {
        var bytes = TileTestData.Bytes(8, bits, 1);
        var entry = new NxtTile(bytes, bits);
        entry.Data.Should().SequenceEqual(bytes);
        entry.Length.Should().Equal(64 * bits / 8);
        entry.BitsPerPixel.Should().Equal(bits);
        entry.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
    }
    [TestCase(0)]
    [TestCase(2)]
    [TestCase(16)]
    public void Constructor_InvalidBitDepth(int bits) =>
        AssertThat.Invoking(() => new NxtTile(new byte[64], bits)).Should().Throw<ArgumentOutOfRangeException>();
    [TestCase(0)]
    [TestCase(31)]
    [TestCase(33)]
    public void Constructor_InvalidLength(int length) =>
        AssertThat.Invoking(() => new NxtTile(new byte[length], 4)).Should().Throw<InvalidDataException>();
}