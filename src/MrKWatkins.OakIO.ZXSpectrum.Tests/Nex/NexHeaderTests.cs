using MrKWatkins.OakIO.ZXSpectrum.Nex;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Nex;

public sealed class NexHeaderTests
{
    [Test]
    public void DefaultConstructor_SetsMagicAndVersion()
    {
        var header = new NexHeader();
        header.Magic.Should().Equal("Next");
        header.VersionString.Should().Equal("V1.2");
        header.Version.Should().Equal(NexVersion.V12);
    }

    [Test]
    public void Version_V13()
    {
        var header = new NexHeader { VersionString = "V1.3" };
        header.Version.Should().Equal(NexVersion.V13);
    }

    [Test]
    public void RamRequired()
    {
        var header = new NexHeader { RamRequired = NexRamRequired.Ram1792K };
        header.RamRequired.Should().Equal(NexRamRequired.Ram1792K);
    }

    [Test]
    public void NumBanksToLoad()
    {
        var header = new NexHeader { NumBanksToLoad = 3 };
        header.NumBanksToLoad.Should().Equal(3);
    }

    [Test]
    public void LoadScreenFlags()
    {
        var header = new NexHeader
        {
            HasLayer2Screen = true,
            HasUlaScreen = false,
            HasLoResScreen = true
        };

        header.HasLayer2Screen.Should().BeTrue();
        header.HasUlaScreen.Should().BeFalse();
        header.HasLoResScreen.Should().BeTrue();
        header.HasHiResScreen.Should().BeFalse();
        header.HasHiColourScreen.Should().BeFalse();
        header.HasNoPaletteBlock.Should().BeFalse();
    }

    [Test]
    public void BorderColour()
    {
        var header = new NexHeader { BorderColour = ZXColour.Red };
        header.BorderColour.Should().Equal(ZXColour.Red);
    }

    [Test]
    public void SP()
    {
        var header = new NexHeader { SP = 0x5B76 };
        header.SP.Should().Equal(0x5B76);
    }

    [Test]
    public void PC()
    {
        var header = new NexHeader { PC = 0x8000 };
        header.PC.Should().Equal(0x8000);
    }

    [Test]
    public void BankIncluded()
    {
        var header = new NexHeader();
        header.IsBankIncluded(5).Should().BeFalse();

        header.SetBankIncluded(5, true);
        header.IsBankIncluded(5).Should().BeTrue();

        header.SetBankIncluded(5, false);
        header.IsBankIncluded(5).Should().BeFalse();
    }

    [Test]
    public void BankIncluded_ThrowsForInvalidBank()
    {
        var header = new NexHeader();
        AssertThat.Invoking(() => header.IsBankIncluded(-1)).Should().Throw<ArgumentOutOfRangeException>();
        AssertThat.Invoking(() => header.IsBankIncluded(112)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void EntryBank()
    {
        var header = new NexHeader { EntryBank = 7 };
        header.EntryBank.Should().Equal(7);
    }

    [Test]
    public void CoreVersion()
    {
        var header = new NexHeader
        {
            CoreVersionMajor = 3,
            CoreVersionMinor = 0,
            CoreVersionSubMinor = 5
        };

        header.CoreVersionMajor.Should().Equal(3);
        header.CoreVersionMinor.Should().Equal(0);
        header.CoreVersionSubMinor.Should().Equal(5);
    }

    [Test]
    public void V13Properties()
    {
        var header = new NexHeader
        {
            VersionString = "V1.3",
            ExpansionBusEnable = true,
            HasChecksum = true,
            BanksOffset = 0x1234,
            CliBufferAddress = 0x8000,
            CliBufferSize = 256,
            LoadScreens2 = NexLoadScreenMode.Layer2x320x256,
            HasCopperCode = true,
            BigL2BarPosY = 128
        };

        header.ExpansionBusEnable.Should().BeTrue();
        header.HasChecksum.Should().BeTrue();
        header.BanksOffset.Should().Equal((uint)0x1234);
        header.CliBufferAddress.Should().Equal(0x8000);
        header.CliBufferSize.Should().Equal(256);
        header.LoadScreens2.Should().Equal(NexLoadScreenMode.Layer2x320x256);
        header.HasCopperCode.Should().BeTrue();
        header.BigL2BarPosY.Should().Equal(128);
    }
}