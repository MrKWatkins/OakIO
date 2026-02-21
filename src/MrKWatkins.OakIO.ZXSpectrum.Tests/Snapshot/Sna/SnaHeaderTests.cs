using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Sna;

public sealed class SnaHeaderTests
{
    [Test]
    public void Constructor()
    {
        var header = new SnaHeader();
        header.AsReadOnlySpan().Length.Should().Equal(27);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void IFF2()
    {
        var bytes = new byte[27];
        var header = new SnaHeader(bytes);

        header.IFF2.Should().BeFalse();
        header.IFF2 = true;

        var expected = new byte[27];
        expected[19] = 0x04;
        bytes.Should().SequenceEqual(expected);

        header.IFF2 = false;
        expected[19] = 0x00;
        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    public void InterruptMode([Values(0, 1, 2)] byte interruptMode)
    {
        var bytes = new byte[27];
        var header = new SnaHeader(bytes);

        header.InterruptMode.Should().Equal(0);
        header.InterruptMode = interruptMode;

        var expected = new byte[27];
        expected[25] = interruptMode;
        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    public void BorderColour([Values] ZXColour colour)
    {
        var bytes = new byte[27];
        var header = new SnaHeader(bytes);

        header.BorderColour.Should().Equal(ZXColour.Black);
        header.BorderColour = colour;

        var expected = new byte[27];
        expected[26] = (byte)colour;
        bytes.Should().SequenceEqual(expected);
    }
}