using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources.Scr;

public sealed class ScrBitmapTests
{
    [Test]
    public void Constructor()
    {
        var bytes = ScrTestData.Bytes()[..6144];
        var expected = bytes.ToArray();
        var bitmap = new ScrBitmap(bytes);
        bitmap.Length.Should().Equal(6144);
        bitmap.Data.Should().SequenceEqual(expected);
        bytes[0] ^= 255;
        bitmap.Data.Should().SequenceEqual(expected);
    }
    [TestCase(0)]
    [TestCase(6143)]
    [TestCase(6145)]
    public void Constructor_InvalidLength(int length) =>
        AssertThat.Invoking(() => new ScrBitmap(new byte[length])).Should().Throw<ArgumentException>();

    [TestCase(0, 0, 0, 128)]
    [TestCase(7, 0, 0, 1)]
    [TestCase(8, 0, 1, 128)]
    [TestCase(255, 0, 31, 1)]
    [TestCase(0, 1, 256, 128)]
    [TestCase(0, 7, 1792, 128)]
    [TestCase(0, 8, 32, 128)]
    [TestCase(0, 63, 2016, 128)]
    [TestCase(0, 64, 2048, 128)]
    [TestCase(0, 127, 4064, 128)]
    [TestCase(0, 128, 4096, 128)]
    [TestCase(255, 191, 6143, 1)]
    public void GetPixel(int x, int y, int offset, byte mask)
    {
        var bytes = new byte[6144];
        bytes[offset] = mask;
        new ScrBitmap(bytes).GetPixel(x, y).Should().BeTrue();
        bytes[offset] = (byte)~mask;
        new ScrBitmap(bytes).GetPixel(x, y).Should().BeFalse();
    }
    [TestCase(-1, 0)]
    [TestCase(256, 0)]
    [TestCase(257, 0)]
    [TestCase(0, -1)]
    [TestCase(0, 192)]
    [TestCase(0, 193)]
    public void GetPixel_Invalid(int x, int y) =>
        AssertThat.Invoking(() => new ScrBitmap(new byte[6144]).GetPixel(x, y)).Should().Throw<ArgumentOutOfRangeException>();
}