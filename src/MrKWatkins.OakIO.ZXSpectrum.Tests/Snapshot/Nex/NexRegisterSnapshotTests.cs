using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Nex;

public sealed class NexRegisterSnapshotTests
{
    [TestCase("AF")]
    [TestCase("BC")]
    [TestCase("DE")]
    [TestCase("HL")]
    [TestCase("IX")]
    [TestCase("IY")]
    [TestCase("IR")]
    // ReSharper disable once InconsistentNaming
    public void Register_NotStored(string register)
    {
        var registers = new NexRegisterSnapshot(new byte[16]);

        var property = typeof(RegisterSnapshot).GetProperty(register)!;
        property.GetValue(registers).Should().Equal((ushort)0);

        property.SetValue(registers, (ushort)0x1234);
        property.GetValue(registers).Should().Equal((ushort)0);
    }

    [Test]
    public void PC()
    {
        var data = new byte[16];
        var registers = new NexRegisterSnapshot(data);

        registers.PC.Should().Equal(0);

        registers.PC = 0x1234;
        registers.PC.Should().Equal(0x1234);

        data[14].Should().Equal(0x34);
        data[15].Should().Equal(0x12);
    }

    [Test]
    public void SP()
    {
        var data = new byte[16];
        var registers = new NexRegisterSnapshot(data);

        registers.SP.Should().Equal(0);

        registers.SP = 0x1234;
        registers.SP.Should().Equal(0x1234);

        data[12].Should().Equal(0x34);
        data[13].Should().Equal(0x12);
    }

    [Test]
    public void Shadow()
    {
        var registers = new NexRegisterSnapshot(new byte[16]);

        registers.Shadow.Should().BeOfType<NexShadowRegisterSnapshot>();
    }
}