using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Z80Snapshot;

public sealed class Z80SnapshotV2HeaderTests
{
    [Test]
    public void Constructor()
    {
        var header = new Z80SnapshotV2Header();
        header.ExtraLength.Should().Equal(23);
        header.AsReadOnlySpan().Length.Should().Equal(55);
        header.AsReadOnlySpan().ToArray()[30].Should().Equal(23);
    }

    [TestCase((byte)0, HardwareMode.Spectrum48)]
    [TestCase((byte)1, HardwareMode.Spectrum48)]
    [TestCase((byte)2, HardwareMode.SamRam)]
    [TestCase((byte)3, HardwareMode.Spectrum128)]
    [TestCase((byte)4, HardwareMode.Spectrum128)]
    public void HardwareMode_Values(byte hardwareModeValue, HardwareMode expected)
    {
        var data = new byte[55];
        data[30] = 23; // ExtraLength.
        data[34] = hardwareModeValue;

        var header = new Z80SnapshotV2Header(data);
        header.HardwareMode.Should().Equal(expected);
    }

    [Test]
    public void HardwareMode_UnsupportedValue()
    {
        var data = new byte[55];
        data[30] = 23;
        data[34] = 5;

        var header = new Z80SnapshotV2Header(data);
        AssertThat.Invoking(() => _ = header.HardwareMode)
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Match(".*not supported in v2.*");
    }
}