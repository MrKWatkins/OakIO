using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources.Scr;

public sealed class ScrAttributesTests
{
    [Test]
    public void Constructor()
    {
        var bytes = ScrTestData.Bytes()[6144..];
        var expected = bytes.ToArray();
        var attributes = new ScrAttributes(bytes);
        attributes.Length.Should().Equal(768);
        attributes.Data.Should().SequenceEqual(expected);
        bytes[0] ^= 255;
        attributes.Data.Should().SequenceEqual(expected);
    }
    [TestCase(0)]
    [TestCase(767)]
    [TestCase(769)]
    public void Constructor_InvalidLength(int length) =>
        AssertThat.Invoking(() => new ScrAttributes(new byte[length])).Should().Throw<ArgumentException>();
    [TestCase(0, 0, 0)]
    [TestCase(31, 0, 31)]
    [TestCase(0, 1, 32)]
    [TestCase(17, 12, 401)]
    [TestCase(31, 23, 767)]
    public void GetAttribute(int x, int y, int offset)
    {
        var bytes = new byte[768];
        bytes[offset] = 239;
        var attribute = new ScrAttributes(bytes).GetAttribute(x, y);
        attribute.Should().Equal(new ScrAttribute(239));
    }
    [TestCase(-1, 0)]
    [TestCase(32, 0)]
    [TestCase(33, 0)]
    [TestCase(0, -1)]
    [TestCase(0, 24)]
    [TestCase(0, 25)]
    public void GetAttribute_Invalid(int x, int y) =>
        AssertThat.Invoking(() => new ScrAttributes(new byte[768]).GetAttribute(x, y)).Should().Throw<ArgumentOutOfRangeException>();
}