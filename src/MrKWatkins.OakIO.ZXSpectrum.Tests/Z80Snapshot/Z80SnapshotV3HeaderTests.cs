using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Z80Snapshot;

public sealed class Z80SnapshotV3HeaderTests
{
    [Test]
    public void Constructor()
    {
        var header = new Z80SnapshotV3Header();
        header.ExtraLength.Should().Equal(55);
        header.AsReadOnlySpan().Length.Should().Equal(87);
        header.AsReadOnlySpan().ToArray()[30].Should().Equal(55);
    }

    [TestCase((byte)0, HardwareMode.Spectrum48)]
    [TestCase((byte)1, HardwareMode.Spectrum48)]
    [TestCase((byte)2, HardwareMode.SamRam)]
    [TestCase((byte)3, HardwareMode.Spectrum48)]
    [TestCase((byte)4, HardwareMode.Spectrum128)]
    [TestCase((byte)5, HardwareMode.Spectrum128)]
    [TestCase((byte)6, HardwareMode.Spectrum128)]
    public void HardwareMode_Values(byte hardwareModeValue, HardwareMode expected)
    {
        var data = new byte[87];
        data[30] = 55; // ExtraLength.
        data[34] = hardwareModeValue;

        var header = new Z80SnapshotV3Header(data);
        header.HardwareMode.Should().Equal(expected);
    }

    [Test]
    public void HardwareMode_UnsupportedValue()
    {
        var data = new byte[87];
        data[30] = 55;
        data[34] = 7;

        var header = new Z80SnapshotV3Header(data);
        AssertThat.Invoking(() => _ = header.HardwareMode)
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Match(".*not supported in v3.*");
    }
}