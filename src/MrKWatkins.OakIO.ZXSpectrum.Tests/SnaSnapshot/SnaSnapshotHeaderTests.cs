using MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.SnaSnapshot;

public sealed class SnaSnapshotHeaderTests
{
    [Test]
    public void Constructor()
    {
        var header = new SnaSnapshotHeader();
        header.AsReadOnlySpan().Length.Should().Equal(27);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void IFF2()
    {
        var bytes = new byte[27];
        var header = new SnaSnapshotHeader(bytes);

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
        var header = new SnaSnapshotHeader(bytes);

        header.InterruptMode.Should().Equal((byte)0);
        header.InterruptMode = interruptMode;

        var expected = new byte[27];
        expected[25] = interruptMode;
        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    public void BorderColour([Values] ZXColour colour)
    {
        var bytes = new byte[27];
        var header = new SnaSnapshotHeader(bytes);

        header.BorderColour.Should().Equal(ZXColour.Black);
        header.BorderColour = colour;

        var expected = new byte[27];
        expected[26] = (byte)colour;
        bytes.Should().SequenceEqual(expected);
    }
}
