using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.Commands.Tests.FileInfo;

public sealed class Z80InfoExtensionsTests
{
    [Test]
    public void ToHardwareInfoSection_V1()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1000;
        var section = z80.ToHardwareInfoSection();
        section.Title.Should().Equal("Hardware");
        // V1 has no HardwareMode property
        section.Properties.Any(p => p.Name == "Hardware Mode").Should().BeFalse();
        section.Properties.Should().HaveCount(7);
    }

    [Test]
    public void ToHardwareInfoSection_V2()
    {
        var z80 = Z80V2File.Create48k(new byte[64 * 1024]);
        var section = z80.ToHardwareInfoSection();
        section.Title.Should().Equal("Hardware");
        // V2 adds HardwareMode
        section.Properties.Any(p => p.Name == "Hardware Mode").Should().BeTrue();
        section.Properties.Should().HaveCount(8);
    }

    [Test]
    public void ToHardwareInfoSection_BorderColour()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1000;
        var section = z80.ToHardwareInfoSection();
        var prop = section.Properties.Single(p => p.Name == "Border Colour");
        prop.Format.Should().Equal("colour");
    }

    [Test]
    public void ToHardwareInfoSection_InterruptMode()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1000;
        var section = z80.ToHardwareInfoSection();
        var prop = section.Properties.Single(p => p.Name == "Interrupt Mode");
        prop.Format.Should().Equal("decimal");
    }

    [Test]
    public void ToHardwareInfoSection_DataCompressed()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1000;
        var section = z80.ToHardwareInfoSection();
        var prop = section.Properties.Single(p => p.Name == "Data Compressed");
        prop.Format.Should().Equal("boolean");
    }
}