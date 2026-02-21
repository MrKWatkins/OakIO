using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

public sealed class PageHeaderTests
{
    [TestCase((byte)4, (ushort)0x8000)]
    [TestCase((byte)5, (ushort)0xC000)]
    [TestCase((byte)8, (ushort)0x4000)]
    public void GetLocation_Spectrum48(byte pageNumber, ushort expectedLocation)
    {
        PageHeader.GetLocation(HardwareMode.Spectrum48, pageNumber).Should().Equal(expectedLocation);
    }

    [TestCase((byte)5, (ushort)0x4000)]
    [TestCase((byte)2, (ushort)0x8000)]
    [TestCase((byte)0, (ushort)0xC000)]
    public void GetLocation_Spectrum128(byte pageNumber, ushort expectedLocation)
    {
        PageHeader.GetLocation(HardwareMode.Spectrum128, pageNumber).Should().Equal(expectedLocation);
    }

    [Test]
    public void GetLocation_Spectrum48_UnsupportedPage()
    {
        AssertThat.Invoking(() => PageHeader.GetLocation(HardwareMode.Spectrum48, 0))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Match(".*Spectrum48.*");
    }

    [Test]
    public void GetLocation_Spectrum128_UnsupportedPage()
    {
        AssertThat.Invoking(() => PageHeader.GetLocation(HardwareMode.Spectrum128, 99))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Match(".*Spectrum128.*");
    }

    [Test]
    public void GetLocation_SamRam_ThrowsNotSupported()
    {
        AssertThat.Invoking(() => PageHeader.GetLocation(HardwareMode.SamRam, 0))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Match(".*SamRam.*");
    }

    [Test]
    public void Constructor_Properties()
    {
        var header = new PageHeader(HardwareMode.Spectrum48, 1000, 4);
        header.HardwareMode.Should().Equal(HardwareMode.Spectrum48);
        header.CompressedLength.Should().Equal(1000);
        header.PageNumber.Should().Equal(4);
        header.Location.Should().Equal(0x8000);
    }
}