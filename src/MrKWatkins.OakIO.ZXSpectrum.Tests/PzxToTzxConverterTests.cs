using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;
using DataBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.DataBlock;
using TzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Tzx.PauseBlock;
using TzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Tzx.PulseSequenceBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests;

public sealed class PzxToTzxConverterTests
{
    private static PzxFile ReadPzx(byte[] data)
    {
        using var stream = new MemoryStream(data);
        return PzxFormat.Instance.Read(stream);
    }

    private static byte[] BuildMinimalPzxHeader()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        return stream.ToArray();
    }

    [Test]
    public void Convert_ReturnsValidTzxHeader()
    {
        var pzx = ReadPzx(BuildMinimalPzxHeader());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        tzx.Header.IsValid.Should().BeTrue();
        tzx.Header.MajorVersion.Should().Equal(1);
        tzx.Header.MinorVersion.Should().Equal(20);
    }

    [Test]
    public void Convert_PzxHeaderBlock_WithTitle_ConvertsToArchiveInfoBlock()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        var infoData = "Monty"u8;
        var size = 2 + infoData.Length;
        stream.Write([(byte)size, (byte)(size >> 8), 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        stream.Write(infoData);

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var archiveInfo = tzx.Blocks[0].Should().BeOfType<ArchiveInfoBlock>().Value;
        archiveInfo.Header.Type.Should().Equal(TzxBlockType.ArchiveInfo);
        archiveInfo.Header.NumberOfTextStrings.Should().Equal(1);
        archiveInfo.Entries.Should().HaveCount(1);
        archiveInfo.Entries[0].Type.Should().Equal(ArchiveInfoType.FullTitle);
        archiveInfo.Entries[0].Text.Should().Equal("Monty");
    }

    [Test]
    public void Convert_PzxHeaderBlock_WithMultipleInfos_ConvertsToArchiveInfoBlock()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        var infoData = "Monty\0Author\0Gremlin"u8;
        var size = 2 + infoData.Length;
        stream.Write([(byte)size, (byte)(size >> 8), 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        stream.Write(infoData);

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var archiveInfo = tzx.Blocks[0].Should().BeOfType<ArchiveInfoBlock>().Value;
        archiveInfo.Entries.Should().HaveCount(2);
        archiveInfo.Entries[0].Type.Should().Equal(ArchiveInfoType.FullTitle);
        archiveInfo.Entries[0].Text.Should().Equal("Monty");
        archiveInfo.Entries[1].Type.Should().Equal(ArchiveInfoType.Authors);
        archiveInfo.Entries[1].Text.Should().Equal("Gremlin");
    }

    [Test]
    public void Convert_PzxHeaderBlock_NoInfo_NoArchiveInfoBlock()
    {
        var pzx = ReadPzx(BuildMinimalPzxHeader());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        tzx.Blocks.Should().BeEmpty();
    }

    [Test]
    public void Convert_PulseSequenceBlock_SinglePulses_ConvertsToPulseSequenceBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        stream.Write("PULS"u8);
        stream.Write([0x04, 0x00, 0x00, 0x00]);
        stream.Write([0x57, 0x03]); // 855.
        stream.Write([0xAE, 0x06]); // 1710.

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var pulseSeq = tzx.Blocks[0].Should().BeOfType<TzxPulseSequenceBlock>().Value;
        pulseSeq.Header.NumberOfPulses.Should().Equal(2);
        pulseSeq.Pulses.ToArray().Should().SequenceEqual(new ushort[] { 855, 1710 });
    }

    [Test]
    public void Convert_PulseSequenceBlock_RepeatedPulses_ConvertsToPureToneBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        // PULS block with a repeated pulse: count=3223, duration=2168.
        // Encoded as: 0x8000 | 3223 = 0x8C97, then 0x0878.
        stream.Write("PULS"u8);
        stream.Write([0x04, 0x00, 0x00, 0x00]);
        stream.Write([0x97, 0x8C]); // 0x8000 | 3223 = count marker.
        stream.Write([0x78, 0x08]); // 2168 = duration.

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var pureTone = tzx.Blocks[0].Should().BeOfType<PureToneBlock>().Value;
        pureTone.Header.LengthOfPulse.Should().Equal(2168);
        pureTone.Header.NumberOfPulses.Should().Equal(3223);
    }

    [Test]
    public void Convert_PulseSequenceBlock_MixedPulses_ConvertsToPureToneAndPulseSequence()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        // PULS block with: 3223x2168 (tone), then 667, 735 (single pulses).
        stream.Write("PULS"u8);
        stream.Write([0x08, 0x00, 0x00, 0x00]);
        stream.Write([0x97, 0x8C]); // count=3223.
        stream.Write([0x78, 0x08]); // duration=2168.
        stream.Write([0x9B, 0x02]); // 667 (single).
        stream.Write([0xDF, 0x02]); // 735 (single).

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        tzx.Blocks.Should().HaveCount(2);

        var pureTone = tzx.Blocks[0].Should().BeOfType<PureToneBlock>().Value;
        pureTone.Header.LengthOfPulse.Should().Equal(2168);
        pureTone.Header.NumberOfPulses.Should().Equal(3223);

        var pulseSeq = tzx.Blocks[1].Should().BeOfType<TzxPulseSequenceBlock>().Value;
        pulseSeq.Pulses.ToArray().Should().SequenceEqual(new ushort[] { 667, 735 });
    }

    [Test]
    public void Convert_DataBlock_ConvertsToPureDataBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        // DATA block: 16 bits, 2 pulses per sequence, symmetric.
        stream.Write("DATA"u8);
        var dataSize = 8 + 4 + 4 + 2; // header fields + 2*2 zero seq + 2*2 one seq + 2 data bytes.
        stream.Write([(byte)dataSize, 0x00, 0x00, 0x00]);
        stream.Write([0x10, 0x00, 0x00, 0x00]); // SizeInBits=16, InitialPulseLevel=false.
        stream.Write([0xB1, 0x03]); // Tail = 945.
        stream.WriteByte(0x02); // NumberOfPulseInZeroBitSequence = 2.
        stream.WriteByte(0x02); // NumberOfPulseInOneBitSequence = 2.
        stream.Write([0x57, 0x03, 0x57, 0x03]); // ZeroBitPulseSequence = [855, 855].
        stream.Write([0xAE, 0x06, 0xAE, 0x06]); // OneBitPulseSequence = [1710, 1710].
        stream.Write([0xAA, 0x55]); // Data bytes.

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var pureData = tzx.Blocks[0].Should().BeOfType<PureDataBlock>().Value;
        pureData.Header.Type.Should().Equal(TzxBlockType.PureData);
        pureData.Header.TStatesInZeroBitPulse.Should().Equal(855);
        pureData.Header.TStatesInOneBitPulse.Should().Equal(1710);
        pureData.Header.UsedBitsInLastByte.Should().Equal(8);
        pureData.Header.PauseAfterBlockMs.Should().Equal(0);
        pureData.Header.BlockLength.Should().Equal(2);
    }

    [Test]
    public void Convert_DataBlock_WithExtraBits_SetsUsedBitsInLastByte()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        // DATA block: 11 bits (1 byte + 3 extra bits).
        stream.Write("DATA"u8);
        var dataSize = 8 + 2 + 2 + 2; // 14
        stream.Write([(byte)dataSize, 0x00, 0x00, 0x00]);
        stream.Write([0x0B, 0x00, 0x00, 0x00]); // SizeInBits=11.
        stream.Write([0x00, 0x00]); // Tail = 0.
        stream.WriteByte(0x01);
        stream.WriteByte(0x01);
        stream.Write([0x57, 0x03]); // 855.
        stream.Write([0xAE, 0x06]); // 1710.
        stream.Write([0xAA, 0x55]);

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var pureData = tzx.Blocks[0].Should().BeOfType<PureDataBlock>().Value;
        pureData.Header.UsedBitsInLastByte.Should().Equal(3);
        pureData.Header.BlockLength.Should().Equal(2);
    }

    [Test]
    public void Convert_PauseBlock_ConvertsToPauseBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        // PAUS: Duration = 3500000 T-states = 1000ms.
        stream.Write("PAUS"u8);
        stream.Write([0x04, 0x00, 0x00, 0x00]);
        stream.Write([0x60, 0x6D, 0x35, 0x00]); // 3500000 = 0x0035_6D60.

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var pause = tzx.Blocks[0].Should().BeOfType<TzxPauseBlock>().Value;
        pause.Header.PauseMs.Should().Equal(1000);
    }

    [Test]
    public void Convert_StopBlock_Only48k_ConvertsToStopTheTapeIf48KBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        stream.Write("STOP"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.Write([0x01, 0x00]); // Only48k = true.

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        tzx.Blocks[0].Should().BeOfType<StopTheTapeIf48KBlock>();
    }

    [Test]
    public void Convert_StopBlock_NotOnly48k_ConvertsToPauseBlockWithZeroDuration()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        stream.Write("STOP"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.Write([0x00, 0x00]); // Only48k = false.

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var pause = tzx.Blocks[0].Should().BeOfType<TzxPauseBlock>().Value;
        pause.Header.PauseMs.Should().Equal(0);
    }

    [Test]
    public void Convert_BrowsePointBlock_ConvertsToTextDescriptionBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalPzxHeader());

        stream.Write("BRWS"u8);
        stream.Write([0x05, 0x00, 0x00, 0x00]);
        stream.Write("Hello"u8);

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        var text = tzx.Blocks[0].Should().BeOfType<TextDescriptionBlock>().Value;
        text.Header.Type.Should().Equal(TzxBlockType.TextDescription);
        text.Text.Should().Equal("Hello");
    }

    [Test]
    public void Convert_AllBlockTypes()
    {
        using var stream = new MemoryStream();

        // PZXT Header with title.
        stream.Write("PZXT"u8);
        var infoData = "Monty"u8;
        var headerSize = 2 + infoData.Length;
        stream.Write([(byte)headerSize, (byte)(headerSize >> 8), 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        stream.Write(infoData);

        // PULS with mixed pulses.
        stream.Write("PULS"u8);
        stream.Write([0x08, 0x00, 0x00, 0x00]);
        stream.Write([0x97, 0x8C]); // count=3223.
        stream.Write([0x78, 0x08]); // duration=2168.
        stream.Write([0x9B, 0x02]); // 667.
        stream.Write([0xDF, 0x02]); // 735.

        // DATA block.
        stream.Write("DATA"u8);
        stream.Write([0x0E, 0x00, 0x00, 0x00]);
        stream.Write([0x10, 0x00, 0x00, 0x00]); // 16 bits.
        stream.Write([0x00, 0x00]); // Tail.
        stream.WriteByte(0x01);
        stream.WriteByte(0x01);
        stream.Write([0x57, 0x03]); // 855.
        stream.Write([0xAE, 0x06]); // 1710.
        stream.Write([0xAA, 0x55]);

        // PAUS.
        stream.Write("PAUS"u8);
        stream.Write([0x04, 0x00, 0x00, 0x00]);
        stream.Write([0xA0, 0x86, 0x01, 0x00]); // 100000 T-states.

        // BRWS.
        stream.Write("BRWS"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.Write("Hi"u8);

        // STOP.
        stream.Write("STOP"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.Write([0x01, 0x00]);

        var pzx = ReadPzx(stream.ToArray());
        var converter = new PzxToTzxConverter();

        var tzx = converter.Convert(pzx);

        // Expected: ArchiveInfo, PureTone, PulseSequence, PureData, Pause, TextDescription, StopTheTapeIf48K.
        tzx.Blocks.Should().HaveCount(7);
        tzx.Blocks[0].Should().BeOfType<ArchiveInfoBlock>();
        tzx.Blocks[1].Should().BeOfType<PureToneBlock>();
        tzx.Blocks[2].Should().BeOfType<TzxPulseSequenceBlock>();
        tzx.Blocks[3].Should().BeOfType<PureDataBlock>();
        tzx.Blocks[4].Should().BeOfType<TzxPauseBlock>();
        tzx.Blocks[5].Should().BeOfType<TextDescriptionBlock>();
        tzx.Blocks[6].Should().BeOfType<StopTheTapeIf48KBlock>();
    }
}
