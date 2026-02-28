using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

namespace MrKWatkins.OakIO.Commands.Tests.FileInfo;

public sealed class SnaInfoExtensionsTests
{
    [Test]
    public void ToHardwareInfoSection()
    {
        var sna = Sna48kFile.Create(new byte[64 * 1024]);
        var section = sna.ToHardwareInfoSection();
        section.Title.Should().Equal("Hardware");
        section.Properties.Should().HaveCount(3);
    }

    [Test]
    public void ToHardwareInfoSection_BorderColour()
    {
        var sna = Sna48kFile.Create(new byte[64 * 1024]);
        var section = sna.ToHardwareInfoSection();
        var prop = section.Properties.Single(p => p.Name == "Border Colour");
        prop.Format.Should().Equal("colour");
    }

    [Test]
    public void ToHardwareInfoSection_InterruptMode()
    {
        var sna = Sna48kFile.Create(new byte[64 * 1024]);
        var section = sna.ToHardwareInfoSection();
        var prop = section.Properties.Single(p => p.Name == "Interrupt Mode");
        prop.Value.Should().Equal("0");
        prop.Format.Should().Equal("decimal");
    }

    [Test]
    public void ToHardwareInfoSection_IFF2()
    {
        var sna = Sna48kFile.Create(new byte[64 * 1024]);
        var section = sna.ToHardwareInfoSection();
        var prop = section.Properties.Single(p => p.Name == "IFF2");
        prop.Format.Should().Equal("boolean");
    }
}