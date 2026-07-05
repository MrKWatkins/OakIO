using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Nex;

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
            HasLoResScreen = true,
            HasHiResScreen = true,
            HasHiColourScreen = true,
            HasNoPaletteBlock = true,
            LoadScreenFlags2 = true
        };

        header.HasLayer2Screen.Should().BeTrue();
        header.HasUlaScreen.Should().BeFalse();
        header.HasLoResScreen.Should().BeTrue();
        header.HasHiResScreen.Should().BeTrue();
        header.HasHiColourScreen.Should().BeTrue();
        header.HasNoPaletteBlock.Should().BeTrue();
        header.LoadScreenFlags2.Should().BeTrue();
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
    public void NumExtraFiles()
    {
        var header = new NexHeader { NumExtraFiles = 2 };
        header.NumExtraFiles.Should().Equal((ushort)2);
    }

    [Test]
    public void LoadingBarSettings()
    {
        var header = new NexHeader
        {
            LoadingBar = 1,
            LoadingBarColour = 6,
            LoadingDelay = 10,
            StartDelay = 20
        };

        header.LoadingBar.Should().Equal((byte)1);
        header.LoadingBarColour.Should().Equal((byte)6);
        header.LoadingDelay.Should().Equal((byte)10);
        header.StartDelay.Should().Equal((byte)20);
    }

    [Test]
    public void PreserveNextRegisters()
    {
        var header = new NexHeader { PreserveNextRegisters = true };
        header.PreserveNextRegisters.Should().BeTrue();
    }

    [Test]
    public void HiResColour()
    {
        var header = new NexHeader { HiResColour = 4 };
        header.HiResColour.Should().Equal((byte)4);
    }

    [Test]
    public void FileHandleAddress()
    {
        var header = new NexHeader { FileHandleAddress = 0x6000 };
        header.FileHandleAddress.Should().Equal((ushort)0x6000);
    }

    [Test]
    public void TileScreenConfig()
    {
        var header = new NexHeader
        {
            TileScreenConfigReg6B = 1,
            TileScreenConfigReg6C = 2,
            TileScreenConfigReg6E = 3,
            TileScreenConfigReg6F = 4
        };

        header.TileScreenConfigReg6B.Should().Equal((byte)1);
        header.TileScreenConfigReg6C.Should().Equal((byte)2);
        header.TileScreenConfigReg6E.Should().Equal((byte)3);
        header.TileScreenConfigReg6F.Should().Equal((byte)4);
    }

    [Test]
    public void Crc32C()
    {
        var header = new NexHeader { Crc32C = 0xDEADBEEF };
        header.Crc32C.Should().Equal(0xDEADBEEFu);
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
        header.BanksOffset.Should().Equal(0x1234);
        header.CliBufferAddress.Should().Equal(0x8000);
        header.CliBufferSize.Should().Equal(256);
        header.LoadScreens2.Should().Equal(NexLoadScreenMode.Layer2x320x256);
        header.HasCopperCode.Should().BeTrue();
        header.BigL2BarPosY.Should().Equal(128);
    }
}