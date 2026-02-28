using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

namespace MrKWatkins.OakIO.Commands.Tests.FileInfo;

public sealed class NexInfoExtensionsTests : CommandsTestFixture
{
    [Test]
    public void ToInfoSections_HeaderSection()
    {
        using var nexFile = CreateNexFile();
        var nex = NexFormat.Instance.Read(nexFile.Bytes);
        var sections = nex.ToInfoSections();
        var header = sections.Single(s => s.Title == "Header");
        header.Properties.Should().HaveCount(7);
        header.Properties.Any(p => p.Name == "Version").Should().BeTrue();
        header.Properties.Any(p => p.Name == "RAM Required").Should().BeTrue();
        header.Properties.Any(p => p.Name == "Border Colour").Should().BeTrue();
        header.Properties.Any(p => p.Name == "Entry Bank").Should().BeTrue();
        header.Properties.Any(p => p.Name == "Core Version").Should().BeTrue();
        header.Properties.Any(p => p.Name == "Expansion Bus").Should().BeTrue();
        header.Properties.Any(p => p.Name == "Preserve Next Registers").Should().BeTrue();
    }

    [Test]
    public void ToInfoSections_ScreensSection()
    {
        using var nexFile = CreateNexFile();
        var nex = NexFormat.Instance.Read(nexFile.Bytes);
        var sections = nex.ToInfoSections();
        var screens = sections.Single(s => s.Title == "Screens");
        screens.Properties.Should().HaveCount(5);
        screens.Properties.Any(p => p.Name == "Layer 2").Should().BeTrue();
        screens.Properties.Any(p => p.Name == "ULA").Should().BeTrue();
        screens.Properties.Any(p => p.Name == "LoRes").Should().BeTrue();
        screens.Properties.Any(p => p.Name == "HiRes").Should().BeTrue();
        screens.Properties.Any(p => p.Name == "HiColour").Should().BeTrue();
    }

    [Test]
    public void ToInfoSections_NoBanksSection_WhenNoBanks()
    {
        using var nexFile = CreateNexFile();
        var nex = NexFormat.Instance.Read(nexFile.Bytes);
        var sections = nex.ToInfoSections();
        sections.Any(s => s.Title == "Banks").Should().BeFalse();
    }

    [Test]
    public void ToInfoSections_BanksSection_WhenBanksPresent()
    {
        // Bank 5 is first in the NEX bank order, so its data follows immediately after the 512-byte header.
        var data = new byte[512 + 16384];
        "NextV1.2"u8.CopyTo(data);
        data[9] = 1;        // numBanks = 1
        data[18 + 5] = 1;   // bank 5 present
        using var stream = new MemoryStream(data);
        var nex = NexFormat.Instance.Read(stream);
        var sections = nex.ToInfoSections();
        var banks = sections.Single(s => s.Title == "Banks");
        banks.Items.Should().HaveCount(1);
        banks.Items[0].Title.Should().Equal("Bank 5");
    }
}