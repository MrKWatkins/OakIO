using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Z80Snapshot;

public sealed class Z80RegisterSnapshotTests
{
    [TestCase("AF", true, 0, Endian.Big)]
    [TestCase("AF", false, 0, Endian.Big)]
    [TestCase("BC", true, 2)]
    [TestCase("BC", false, 2)]
    [TestCase("DE", true, 13)]
    [TestCase("DE", false, 13)]
    [TestCase("HL", true, 4)]
    [TestCase("HL", false, 4)]
    [TestCase("IX", true, 25)]
    [TestCase("IX", false, 25)]
    [TestCase("IY", true, 23)]
    [TestCase("IY", false, 23)]
    [TestCase("PC", true, 6)]
    [TestCase("PC", false, 32)]
    [TestCase("SP", true, 8)]
    [TestCase("SP", false, 8)]
    public void Register(string register, bool v1, int expectedLocation, Endian expectedEndian = Endian.Little)
    {
        var bytes = new byte[34];
        var registers = new Z80RegisterSnapshot(bytes, v1);

        var property = typeof(RegisterSnapshot).GetProperty(register)!;
        property.GetValue(registers).Should().Equal((ushort)0);

        property.SetValue(registers, (ushort)0x1234);
        property.GetValue(registers).Should().Equal((ushort)0x1234);

        var expected = new byte[34];
        expected[expectedLocation] = (byte)(expectedEndian == Endian.Big ? 0x12 : 0x34);
        expected[expectedLocation + 1] = (byte)(expectedEndian == Endian.Big ? 0x34 : 0x12);

        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    // ReSharper disable once InconsistentNaming
    public void IR([Values] bool v1)
    {
        var bytes = new byte[34];
        RegisterSnapshot registers = new Z80RegisterSnapshot(bytes, v1);

        registers.IR.Should().Equal(0);

        registers.IR = 0xFEDC;
        registers.IR.Should().Equal(0xFEDC);

        var expected = new byte[34];
        expected[10] = 0xDC;
        expected[11] = 0x7E;
        expected[12] = 0x01;

        bytes.Should().SequenceEqual(expected);

        bytes[11] = 0xFE;
        bytes[12] = 0xFF;
        registers.IR.Should().Equal(0xFEDC);

        registers.IR = 0xFEDC;

        expected[11] = 0xFE;
        expected[12] = 0xFF;
        bytes.Should().SequenceEqual(expected);
    }
}