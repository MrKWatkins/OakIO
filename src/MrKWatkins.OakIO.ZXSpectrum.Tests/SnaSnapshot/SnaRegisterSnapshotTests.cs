using MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.SnaSnapshot;

public sealed class SnaRegisterSnapshotTests
{
    [TestCase("AF", 21)]
    [TestCase("BC", 13)]
    [TestCase("DE", 11)]
    [TestCase("HL", 9)]
    [TestCase("IX", 17)]
    [TestCase("IY", 15)]
    [TestCase("SP", 23)]
    public void Register(string register, int expectedLocation)
    {
        var headerBytes = new byte[27];
        var footerData = new byte[2];
        var registers = new SnaRegisterSnapshot(headerBytes, footerData);

        var property = typeof(RegisterSnapshot).GetProperty(register)!;
        property.GetValue(registers).Should().Equal((ushort)0);

        property.SetValue(registers, (ushort)0x1234);
        property.GetValue(registers).Should().Equal((ushort)0x1234);

        var expected = new byte[27];
        expected[expectedLocation] = 0x34;
        expected[expectedLocation + 1] = 0x12;

        headerBytes.Should().SequenceEqual(expected);
    }

    [Test]
    public void PC()
    {
        var headerBytes = new byte[27];
        var footerData = new byte[2];
        var registers = new SnaRegisterSnapshot(headerBytes, footerData);

        registers.PC.Should().Equal(0);

        registers.PC = 0x1234;
        registers.PC.Should().Equal(0x1234);

        footerData[0].Should().Equal(0x34);
        footerData[1].Should().Equal(0x12);
    }

    [Test]
    // ReSharper disable once InconsistentNaming
    public void IR()
    {
        var headerBytes = new byte[27];
        var footerData = new byte[2];
        RegisterSnapshot registers = new SnaRegisterSnapshot(headerBytes, footerData);

        registers.IR.Should().Equal(0);

        registers.IR = 0xFEDC;
        registers.IR.Should().Equal(0xFEDC);

        var expected = new byte[27];
        expected[0] = 0xDC;
        expected[20] = 0xFE;

        headerBytes.Should().SequenceEqual(expected);
    }
}