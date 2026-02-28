using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.Commands.Tests.FileInfo;

public sealed class SnapshotInfoExtensionsTests : CommandsTestFixture
{
    [Test]
    public void ToInfoSections_Z80File()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1000;
        var sections = z80.ToInfoSections();
        sections.Any(s => s.Title == "Hardware").Should().BeTrue();
        sections.Any(s => s.Title == "Registers").Should().BeTrue();
        sections.Any(s => s.Title == "Shadow Registers").Should().BeTrue();
    }

    [Test]
    public void ToInfoSections_SnaFile()
    {
        var sna = Sna48kFile.Create(new byte[64 * 1024]);
        var sections = sna.ToInfoSections();
        sections.Any(s => s.Title == "Hardware").Should().BeTrue();
        sections.Any(s => s.Title == "Registers").Should().BeTrue();
        sections.Any(s => s.Title == "Shadow Registers").Should().BeTrue();
    }

    [Test]
    public void ToInfoSections_NexFile()
    {
        using var nexFile = CreateNexFile();
        var nex = (ZXSpectrumSnapshotFile)NexFormat.Instance.Read(nexFile.Bytes);
        var sections = nex.ToInfoSections();
        sections.Any(s => s.Title == "Header").Should().BeTrue();
        sections.Any(s => s.Title == "Registers").Should().BeTrue();
        // NEX files do not contain shadow register data
        sections.Any(s => s.Title == "Shadow Registers").Should().BeFalse();
    }

    [Test]
    public void ToInfoSection_RegisterSnapshot()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1000;
        var section = z80.Registers.ToInfoSection();
        section.Title.Should().Equal("Registers");
        section.Properties.Should().HaveCount(9);
        section.Properties.Any(p => p.Name == "AF").Should().BeTrue();
        section.Properties.Any(p => p.Name == "BC").Should().BeTrue();
        section.Properties.Any(p => p.Name == "DE").Should().BeTrue();
        section.Properties.Any(p => p.Name == "HL").Should().BeTrue();
        section.Properties.Any(p => p.Name == "IX").Should().BeTrue();
        section.Properties.Any(p => p.Name == "IY").Should().BeTrue();
        section.Properties.Any(p => p.Name == "PC").Should().BeTrue();
        section.Properties.Any(p => p.Name == "SP").Should().BeTrue();
        section.Properties.Any(p => p.Name == "IR").Should().BeTrue();
    }

    [Test]
    public void ToInfoSection_RegisterSnapshot_FormatsAsHex()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1234;
        var section = z80.Registers.ToInfoSection();
        var pc = section.Properties.Single(p => p.Name == "PC");
        pc.Value.Should().Equal("0x1234");
        pc.Format.Should().Equal("hex");
    }

    [Test]
    public void ToInfoSection_ShadowRegisterSnapshot()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1000;
        var section = z80.Registers.Shadow.ToInfoSection();
        section.Title.Should().Equal("Shadow Registers");
        section.Properties.Should().HaveCount(4);
        section.Properties.Any(p => p.Name == "AF'").Should().BeTrue();
        section.Properties.Any(p => p.Name == "BC'").Should().BeTrue();
        section.Properties.Any(p => p.Name == "DE'").Should().BeTrue();
        section.Properties.Any(p => p.Name == "HL'").Should().BeTrue();
    }

    [Test]
    public void ToInfoSection_ShadowRegisterSnapshot_FormatsAsHex()
    {
        var z80 = Z80V1File.Create48k(new byte[48 * 1024]);
        z80.Header.Registers.PC = 0x1000;
        var section = z80.Registers.Shadow.ToInfoSection();
        section.Properties.All(p => p.Format == "hex").Should().BeTrue();
    }
}