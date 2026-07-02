using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

public sealed class RzxHeaderTests
{
    [Test]
    public void Constructor_Default()
    {
        var header = new RzxHeader();

        header.Signature.Should().Equal("RZX!");
        header.MajorVersion.Should().Equal((byte)0);
        header.MinorVersion.Should().Equal((byte)13);
        header.Flags.Should().Equal(0U);
        header.IsValid.Should().BeTrue();
        header.IsSupportedVersion.Should().BeTrue();
    }

    [Test]
    public void Constructor_Flags()
    {
        var header = new RzxHeader(0x1234U);

        header.Flags.Should().Equal(0x1234U);
    }

    [Test]
    public void Constructor_Default_Bytes()
    {
        // "RZX!", major 0, minor 13, flags 0.
        new RzxHeader().Data.Should().SequenceEqual(0x52, 0x5A, 0x58, 0x21, 0x00, 0x0D, 0x00, 0x00, 0x00, 0x00);
    }

    [Test]
    public void Constructor_Flags_Bytes()
    {
        // Flags 0x04030201 little-endian.
        new RzxHeader(0x04030201U).Data.Should().SequenceEqual(0x52, 0x5A, 0x58, 0x21, 0x00, 0x0D, 0x01, 0x02, 0x03, 0x04);
    }

    [Test]
    public void IsValid_False()
    {
        var data = new byte[10];
        "NOPE"u8.CopyTo(data);

        new RzxHeader(data).IsValid.Should().BeFalse();
    }

    [TestCase((byte)0, (byte)11, false)]
    [TestCase((byte)0, (byte)12, true)]
    [TestCase((byte)0, (byte)13, true)]
    [TestCase((byte)1, (byte)13, false)]
    public void IsSupportedVersion(byte major, byte minor, bool expected)
    {
        var data = new byte[10];
        "RZX!"u8.CopyTo(data);
        data[4] = major;
        data[5] = minor;

        new RzxHeader(data).IsSupportedVersion.Should().Equal(expected);
    }
}