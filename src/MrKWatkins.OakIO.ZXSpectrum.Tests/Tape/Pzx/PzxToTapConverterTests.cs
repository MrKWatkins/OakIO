using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using TapDataBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Tap.DataBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Pzx;

public sealed class PzxToTapConverterTests
{
    [Test]
    public void Convert()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        var result = new PzxToTapConverter().Convert(pzx);

        result.Blocks.Should().HaveCount(2);
        result.Blocks[0].Should().BeOfType<HeaderBlock>();
        result.Blocks[1].Should().BeOfType<TapDataBlock>();
    }

    [Test]
    public void Convert_PreservesHeaderBlockData()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        var result = new PzxToTapConverter().Convert(pzx);

        var header = (HeaderBlock)result.Blocks[0];
        header.HeaderType.Should().Equal(TapHeaderType.Code);
        header.Filename.Should().Equal("test");
        header.DataBlockLength.Should().Equal(2);
        header.Parameter1.Should().Equal(0x8000);
    }

    [Test]
    public void Convert_PreservesDataBlockData()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        var result = new PzxToTapConverter().Convert(pzx);

        var data = (TapDataBlock)result.Blocks[1];
        data.Data.Should().SequenceEqual(0xF3, 0xAF);
    }

    [Test]
    public void Convert_PreservesChecksum()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var originalChecksum = tap.Blocks[1].Trailer.Checksum;
        var pzx = new TapToPzxConverter().Convert(tap);

        var result = new PzxToTapConverter().Convert(pzx);

        result.Blocks[1].Trailer.Checksum.Should().Equal(originalChecksum);
    }

    [Test]
    public void Convert_RoundTrip()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        var result = new PzxToTapConverter().Convert(pzx);

        result.Blocks.Should().HaveCount(tap.Blocks.Count);
        for (var i = 0; i < tap.Blocks.Count; i++)
        {
            result.Blocks[i].Data.Should().SequenceEqual(tap.Blocks[i].Data);
            result.Blocks[i].Trailer.Checksum.Should().Equal(tap.Blocks[i].Trailer.Checksum);
        }
    }

    [Test]
    public void Convert_RoundTrip_ProgramBlock()
    {
        var tap = TapFile.CreateLoader("loader", 0x8000, (0x8000, [0xF3, 0xAF]));
        var pzx = new TapToPzxConverter().Convert(tap);

        var result = new PzxToTapConverter().Convert(pzx);

        result.Blocks.Should().HaveCount(tap.Blocks.Count);
        var header = (HeaderBlock)result.Blocks[0];
        header.HeaderType.Should().Equal(TapHeaderType.Program);
    }

    [Test]
    public void Convert_SkipsPzxHeaderBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        // The PZX header block is already in the file from TapToPzxConverter.
        var result = new PzxToTapConverter().Convert(pzx);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_SkipsPauseBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        // The PZX already contains pause blocks from TapToPzxConverter.
        var result = new PzxToTapConverter().Convert(pzx);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_SkipsBrowsePointBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        // Add a BrowsePointBlock.
        var headerData = new byte[4];
        headerData[0] = 0x04;
        headerData[1] = 0x00;
        headerData[2] = 0x00;
        headerData[3] = 0x00;
        var bodyData = "test"u8.ToArray();
        var browseBlock = new BrowsePointBlock(headerData, bodyData);

        var blocks = new List<PzxBlock>(pzx.Blocks) { browseBlock };
        var pzxWithBrowse = new PzxFile(blocks);

        var result = new PzxToTapConverter().Convert(pzxWithBrowse);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_SkipsStopBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        // Add a StopBlock.
        var headerData = new byte[4];
        headerData[0] = 0x00;
        headerData[1] = 0x00;
        headerData[2] = 0x00;
        headerData[3] = 0x00;
        var stopBlock = new StopBlock(headerData);

        var blocks = new List<PzxBlock>(pzx.Blocks) { stopBlock };
        var pzxWithStop = new PzxFile(blocks);

        var result = new PzxToTapConverter().Convert(pzxWithStop);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_SkipsPulseSequenceBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        // PZX files from TapToPzxConverter already contain PulseSequenceBlocks for pilot tones.
        var result = new PzxToTapConverter().Convert(pzx);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_ThrowsForNoDataBlocks()
    {
        var headerData = new byte[4];
        headerData[0] = 0x00;
        headerData[1] = 0x00;
        headerData[2] = 0x00;
        headerData[3] = 0x00;
        var stopBlock = new StopBlock(headerData);

        var pzx = new PzxFile([stopBlock]);

        AssertThat.Invoking(() => new PzxToTapConverter().Convert(pzx))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Contain("no data blocks");
    }

    [Test]
    public void Convert_NonHeaderFlagByte_CreatesDataBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        var result = new PzxToTapConverter().Convert(pzx);

        // The second block has flag 0xFF, which should become a DataBlock.
        result.Blocks[1].Should().BeOfType<TapDataBlock>();
    }

    [Test]
    public void Convert_MultipleDataBlocks()
    {
        var tap = TapFile.CreateLoader("test", 0x8000, (0x8000, [0xF3, 0xAF]));
        var pzx = new TapToPzxConverter().Convert(tap);

        var result = new PzxToTapConverter().Convert(pzx);

        // Loader: header + data + code header + code data = 4 blocks.
        result.Blocks.Should().HaveCount(tap.Blocks.Count);
    }
}