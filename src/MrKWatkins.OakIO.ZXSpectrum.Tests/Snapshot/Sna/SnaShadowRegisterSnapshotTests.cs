using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Sna;

public sealed class SnaShadowRegisterSnapshotTests
{
    [TestCase("AF", 7)]
    [TestCase("BC", 5)]
    [TestCase("DE", 3)]
    [TestCase("HL", 1)]
    public void Register(string register, int expectedLocation)
    {
        var bytes = new byte[27];
        var shadow = new SnaShadowRegisterSnapshot(bytes);

        var property = typeof(ShadowRegisterSnapshot).GetProperty(register)!;
        property.GetValue(shadow).Should().Equal((ushort)0);

        property.SetValue(shadow, (ushort)0x1234);
        property.GetValue(shadow).Should().Equal((ushort)0x1234);

        var expected = new byte[27];
        expected[expectedLocation] = 0x34;
        expected[expectedLocation + 1] = 0x12;

        bytes.Should().SequenceEqual(expected);
    }
}