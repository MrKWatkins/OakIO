using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Z80Snapshot;

public sealed class Z80ShadowRegisterSnapshotTests
{
    [TestCase("AF", 21, Endian.Big)]
    [TestCase("BC", 15)]
    [TestCase("DE", 17)]
    [TestCase("HL", 19)]
    public void Register(string register, int expectedLocation, Endian expectedEndian = Endian.Little)
    {
        var bytes = new byte[34];
        var registers = new Z80ShadowRegisterSnapshot(bytes);

        var property = typeof(ShadowRegisterSnapshot).GetProperty(register)!;
        property.GetValue(registers).Should().Equal((ushort)0);

        property.SetValue(registers, (ushort)0x1234);
        property.GetValue(registers).Should().Equal((ushort)0x1234);

        var expected = new byte[34];
        expected[expectedLocation] = (byte)(expectedEndian == Endian.Big ? 0x12 : 0x34);
        expected[expectedLocation + 1] = (byte)(expectedEndian == Endian.Big ? 0x34 : 0x12);

        bytes.Should().SequenceEqual(expected);
    }
}