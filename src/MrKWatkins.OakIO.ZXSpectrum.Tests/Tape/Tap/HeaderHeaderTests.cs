using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class HeaderHeaderTests
{
    [Test]
    public void Constructor_ValidLength()
    {
        var header = new HeaderHeader(19);
        header.BlockFlagAndChecksumLength.Should().Equal(19);
        header.BlockLength.Should().Equal(17);
    }

    [Test]
    public void Constructor_InvalidLength()
    {
        AssertThat.Invoking(() => new HeaderHeader(20))
            .Should().ThrowArgumentOutOfRangeException("Header + flag + checksum length must be 19.", "blockFlagAndChecksumLength", (ushort)20);
    }
}