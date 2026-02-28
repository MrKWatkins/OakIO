using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tzx;

public sealed class TzxToTapConverterTests
{
    [Test]
    public void Convert()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        var result = new TzxToTapConverter().Convert(tzx);

        result.Blocks.Should().HaveCount(2);
        result.Blocks[0].Should().BeOfType<HeaderBlock>();
        result.Blocks[1].Should().BeOfType<DataBlock>();
    }

    [Test]
    public void Convert_PreservesHeaderBlockData()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        var result = new TzxToTapConverter().Convert(tzx);

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
        var tzx = new TapToTzxConverter().Convert(tap);

        var result = new TzxToTapConverter().Convert(tzx);

        var data = (DataBlock)result.Blocks[1];
        data.Data.Should().SequenceEqual(0xF3, 0xAF);
    }

    [Test]
    public void Convert_PreservesChecksum()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var originalChecksum = tap.Blocks[1].Trailer.Checksum;
        var tzx = new TapToTzxConverter().Convert(tap);

        var result = new TzxToTapConverter().Convert(tzx);

        result.Blocks[1].Trailer.Checksum.Should().Equal(originalChecksum);
    }

    [Test]
    public void Convert_RoundTrip()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        var result = new TzxToTapConverter().Convert(tzx);

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
        var tzx = new TapToTzxConverter().Convert(tap);

        var result = new TzxToTapConverter().Convert(tzx);

        result.Blocks.Should().HaveCount(tap.Blocks.Count);
        var header = (HeaderBlock)result.Blocks[0];
        header.HeaderType.Should().Equal(TapHeaderType.Program);
    }

    [Test]
    public void Convert_SkipsArchiveInfoBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        // Add an ArchiveInfoBlock.
        var archiveData = new byte[] { 0x07, 0x00, 0x01, 0x00, 0x04, 0x74, 0x65, 0x73, 0x74 };
        using var stream = new MemoryStream(archiveData);
        var archiveBlock = new ArchiveInfoBlock(stream);

        var blocks = new List<TzxBlock> { archiveBlock };
        blocks.AddRange(tzx.Blocks);
        var tzxWithArchive = new TzxFile(tzx.Header, blocks);

        var result = new TzxToTapConverter().Convert(tzxWithArchive);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_SkipsPauseBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        // Add a PauseBlock between the data blocks.
        var pauseData = new byte[] { 0xE8, 0x03 };
        using var stream = new MemoryStream(pauseData);
        var pauseBlock = new PauseBlock(stream);

        var blocks = new List<TzxBlock>(tzx.Blocks);
        blocks.Insert(1, pauseBlock);
        var tzxWithPause = new TzxFile(tzx.Header, blocks);

        var result = new TzxToTapConverter().Convert(tzxWithPause);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_SkipsTextDescriptionBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        var textData = new byte[] { 0x04, 0x74, 0x65, 0x73, 0x74 };
        using var stream = new MemoryStream(textData);
        var textBlock = new TextDescriptionBlock(stream);

        var blocks = new List<TzxBlock>(tzx.Blocks);
        blocks.Insert(0, textBlock);
        var tzxWithText = new TzxFile(tzx.Header, blocks);

        var result = new TzxToTapConverter().Convert(tzxWithText);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_SkipsGroupBlocks()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        var groupStartData = new byte[] { 0x03, 0x67, 0x72, 0x70 };
        using var startStream = new MemoryStream(groupStartData);
        var groupStart = new GroupStartBlock(startStream);

        using var endStream = new MemoryStream([]);
        var groupEnd = new GroupEndBlock(endStream);

        var blocks = new List<TzxBlock> { groupStart };
        blocks.AddRange(tzx.Blocks);
        blocks.Add(groupEnd);
        var tzxWithGroups = new TzxFile(tzx.Header, blocks);

        var result = new TzxToTapConverter().Convert(tzxWithGroups);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_SkipsStopTheTapeIf48KBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        var stopData = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        using var stream = new MemoryStream(stopData);
        var stopBlock = new StopTheTapeIf48KBlock(stream);

        var blocks = new List<TzxBlock>(tzx.Blocks);
        blocks.Add(stopBlock);
        var tzxWithStop = new TzxFile(tzx.Header, blocks);

        var result = new TzxToTapConverter().Convert(tzxWithStop);

        result.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Convert_ThrowsForTurboSpeedDataBlock()
    {
        // TurboSpeedDataHeader is 18 bytes; set block length = 1 at offset 15 (24-bit).
        var turboData = new byte[19];
        turboData[15] = 0x01;
        using var stream = new MemoryStream(turboData);
        var turboBlock = new TurboSpeedDataBlock(stream);

        var tzx = new TzxFile(new TzxHeader(1, 20), [turboBlock]);

        AssertThat.Invoking(() => new TzxToTapConverter().Convert(tzx))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Contain("TurboSpeedData");
    }

    [Test]
    public void Convert_ThrowsForPureDataBlock()
    {
        // PureDataHeader is 10 bytes; set block length = 1 at offset 7 (24-bit).
        var pureData = new byte[11];
        pureData[4] = 0x08; // UsedBitsInLastByte = 8.
        pureData[7] = 0x01; // BlockLength = 1.
        using var stream = new MemoryStream(pureData);
        var pureBlock = new PureDataBlock(stream);

        var tzx = new TzxFile(new TzxHeader(1, 20), [pureBlock]);

        AssertThat.Invoking(() => new TzxToTapConverter().Convert(tzx))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Contain("PureData");
    }

    [Test]
    public void Convert_ThrowsForPureToneBlock()
    {
        var toneData = new byte[] { 0x78, 0x08, 0x7F, 0x1F };
        using var stream = new MemoryStream(toneData);
        var toneBlock = new PureToneBlock(stream);

        var tzx = new TzxFile(new TzxHeader(1, 20), [toneBlock]);

        AssertThat.Invoking(() => new TzxToTapConverter().Convert(tzx))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Contain("PureTone");
    }

    [Test]
    public void Convert_ThrowsForPulseSequenceBlock()
    {
        var pulseData = new byte[] { 0x02, 0x9B, 0x02, 0xDF, 0x02 };
        using var stream = new MemoryStream(pulseData);
        var pulseBlock = new PulseSequenceBlock(stream);

        var tzx = new TzxFile(new TzxHeader(1, 20), [pulseBlock]);

        AssertThat.Invoking(() => new TzxToTapConverter().Convert(tzx))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Contain("PulseSequence");
    }

    [Test]
    public void Convert_ThrowsForLoopStartBlock()
    {
        var loopData = new byte[] { 0x02, 0x00 };
        using var stream = new MemoryStream(loopData);
        var loopBlock = new LoopStartBlock(stream);

        var tzx = new TzxFile(new TzxHeader(1, 20), [loopBlock]);

        AssertThat.Invoking(() => new TzxToTapConverter().Convert(tzx))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Contain("LoopStart");
    }

    [Test]
    public void Convert_ThrowsForLoopEndBlock()
    {
        using var stream = new MemoryStream([]);
        var loopBlock = new LoopEndBlock(stream);

        var tzx = new TzxFile(new TzxHeader(1, 20), [loopBlock]);

        AssertThat.Invoking(() => new TzxToTapConverter().Convert(tzx))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Contain("LoopEnd");
    }

    [Test]
    public void Convert_ThrowsForNoStandardSpeedBlocks()
    {
        var textData = new byte[] { 0x04, 0x74, 0x65, 0x73, 0x74 };
        using var stream = new MemoryStream(textData);
        var textBlock = new TextDescriptionBlock(stream);

        var tzx = new TzxFile(new TzxHeader(1, 20), [textBlock]);

        AssertThat.Invoking(() => new TzxToTapConverter().Convert(tzx))
            .Should().Throw<NotSupportedException>()
            .Exception.Message.Should().Contain("no standard speed data blocks");
    }

    [Test]
    public void Convert_NonHeaderFlagByte_CreatesDataBlock()
    {
        // A StandardSpeedDataBlock with a flag byte that is not 0x00 (header) and not 0xFF (data).
        // Should still be treated as a data block.
        var bodyData = new byte[] { 0x42, 0x01, 0x02, 0x03 };
        var headerData = new byte[] { 0xE8, 0x03, 0x04, 0x00 };
        var ssdb = new StandardSpeedDataBlock(headerData, bodyData);

        var tzx = new TzxFile(new TzxHeader(1, 20), [ssdb]);

        var result = new TzxToTapConverter().Convert(tzx);

        result.Blocks.Should().HaveCount(1);
        result.Blocks[0].Should().BeOfType<DataBlock>();
    }

    [Test]
    public void Convert_MultipleStandardSpeedBlocks()
    {
        var tap = TapFile.CreateLoader("test", 0x8000, (0x8000, [0xF3, 0xAF]));
        var tzx = new TapToTzxConverter().Convert(tap);

        var result = new TzxToTapConverter().Convert(tzx);

        // Loader: header + data + code header + code data = 4 blocks.
        result.Blocks.Should().HaveCount(tap.Blocks.Count);
    }
}