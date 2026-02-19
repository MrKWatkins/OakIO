using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tap;
using PzxDataBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.DataBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tap;

public sealed class TapToPzxConverterTests
{
    [Test]
    public void Convert_Structure()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var pzx = TapToPzxConverter.Instance.Convert(tap);

        // PZXT + 3 blocks per TAP block (2 TAP blocks = 7 blocks total)
        pzx.Blocks.Should().HaveCount(7);

        pzx.Blocks[0].Should().BeOfType<PzxHeaderBlock>();

        pzx.Blocks[1].Should().BeOfType<PulseSequenceBlock>();
        pzx.Blocks[2].Should().BeOfType<PzxDataBlock>();
        pzx.Blocks[3].Should().BeOfType<PauseBlock>();

        pzx.Blocks[4].Should().BeOfType<PulseSequenceBlock>();
        pzx.Blocks[5].Should().BeOfType<PzxDataBlock>();
        pzx.Blocks[6].Should().BeOfType<PauseBlock>();
    }

    [Test]
    public void Convert_PzxHeader()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var header = (PzxHeaderBlock)pzx.Blocks[0];
        header.Header.MajorVersionNumber.Should().Equal(1);
        header.Header.MinorVersionNumber.Should().Equal(0);
    }

    [Test]
    public void Convert_HeaderBlock_PilotPulses()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var headerPuls = (PulseSequenceBlock)pzx.Blocks[1];
        // Header: 8063 x 2168 + sync1 667 + sync2 735 => 3 pulse entries
        headerPuls.Pulses.Should().HaveCount(3);
        headerPuls.Pulses[0].Count.Should().Equal(8063);
        headerPuls.Pulses[0].Duration.Should().Equal(2168u);
        headerPuls.Pulses[1].Duration.Should().Equal(667u);
        headerPuls.Pulses[2].Duration.Should().Equal(735u);
    }

    [Test]
    public void Convert_DataBlock_PilotPulses()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var dataPuls = (PulseSequenceBlock)pzx.Blocks[4];
        // Data: 3223 x 2168 + sync1 667 + sync2 735 => 3 pulse entries
        dataPuls.Pulses.Should().HaveCount(3);
        dataPuls.Pulses[0].Count.Should().Equal(3223);
        dataPuls.Pulses[0].Duration.Should().Equal(2168u);
    }

    [Test]
    public void Convert_DataBlockEncoding()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var dataBlock = (PzxDataBlock)pzx.Blocks[2];
        dataBlock.Header.InitialPulseLevel.Should().BeTrue();
        dataBlock.Header.Tail.Should().Equal(945);
        dataBlock.ZeroBitPulseSequence.ToArray().Should().SequenceEqual(new ushort[] { 855, 855 });
        dataBlock.OneBitPulseSequence.ToArray().Should().SequenceEqual(new ushort[] { 1710, 1710 });

        // Flag 0x00 + 17 data bytes + checksum = 19 bytes, 152 bits
        dataBlock.Header.SizeInBits.Should().Equal(19u * 8u);
    }

    [Test]
    public void Convert_PauseBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var pause = (PauseBlock)pzx.Blocks[3];
        pause.Header.Duration.Should().Equal(3_500_000u);
    }
}