using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Nex;

public sealed class NexShadowRegisterSnapshotTests
{
    [TestCase("AF")]
    [TestCase("BC")]
    [TestCase("DE")]
    [TestCase("HL")]
    public void Register_NotStored(string register)
    {
        var shadow = new NexShadowRegisterSnapshot();

        var property = typeof(ShadowRegisterSnapshot).GetProperty(register)!;
        property.GetValue(shadow).Should().Equal((ushort)0);

        property.SetValue(shadow, (ushort)0x1234);
        property.GetValue(shadow).Should().Equal((ushort)0);
    }
}