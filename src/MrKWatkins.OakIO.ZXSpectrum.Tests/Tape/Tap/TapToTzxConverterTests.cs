using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class TapToTzxConverterTests
{
    [Test]
    public void Convert_HeaderBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var tzx = new TapToTzxConverter().Convert(tap);

        tzx.Header.MajorVersion.Should().Equal(1);
        tzx.Header.MinorVersion.Should().Equal(20);
        tzx.Header.IsValid.Should().BeTrue();
        tzx.Blocks.Should().HaveCount(2);

        var headerBlock = tzx.Blocks[0].Should().BeOfType<StandardSpeedDataBlock>().Value;
        headerBlock.Header.PauseAfterBlockMs.Should().Equal(1000);
        headerBlock.Header.BlockLength.Should().Equal(19);
        headerBlock.Data[0].Should().Equal(0x00);

        var dataBlock = tzx.Blocks[1].Should().BeOfType<StandardSpeedDataBlock>().Value;
        dataBlock.Header.PauseAfterBlockMs.Should().Equal(1000);
        dataBlock.Header.BlockLength.Should().Equal(4);
        dataBlock.Data[0].Should().Equal(0xFF);
    }

    [Test]
    public void Convert_BlockDataMatchesTap()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var originalDataBlock = (DataBlock)tap.Blocks[1];

        var tzx = new TapToTzxConverter().Convert(tap);

        var tzxDataBlock = (StandardSpeedDataBlock)tzx.Blocks[1];
        tzxDataBlock.Data[0].Should().Equal(0xFF);
        tzxDataBlock.Data[^1].Should().Equal(originalDataBlock.Trailer.Checksum);
    }

    [Test]
    public void Convert_RoundTrip()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);
        tzx.Blocks.Should().HaveCount(2);
        foreach (var block in tzx.Blocks)
        {
            block.Data.Should().NotBeEmpty();
        }
    }
}