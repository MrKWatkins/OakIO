using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tap;
using DataBlock = MrKWatkins.OakIO.ZXSpectrum.Pzx.DataBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Pzx;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class PzxFormatTests
{
    private static byte[] BuildPzxData()
    {
        using var stream = new MemoryStream();

        // PZXT Header block.
        stream.Write("PZXT"u8);
        // Header reads 6 bytes: size (4 bytes LE uint32) + version major/minor.
        // size = SizeOfBlockExcludingTagAndSizeField.
        // SizeOfHeaderExcludingTagAndSizeField = 6 - 4 = 2.
        // BlockLength = size - 2. For data "Test" (4 bytes): size = 2 + 4 = 6.
        stream.Write([0x06, 0x00, 0x00, 0x00]); // Size = 6.
        stream.WriteByte(0x01); // Major version = 1.
        stream.WriteByte(0x00); // Minor version = 0.
        stream.Write("Test"u8); // Info data (title "Test", no null terminator).

        // PULS Pulse Sequence block.
        stream.Write("PULS"u8);
        // PulseSequenceHeader reads 4 bytes (just size).
        // SizeOfHeaderExcludingTagAndSizeField = 0.
        // BlockLength = size. For 2 simple pulses (2 words = 4 bytes): size = 4.
        stream.Write([0x04, 0x00, 0x00, 0x00]); // Size = 4.
        stream.Write([0x57, 0x03]); // Pulse duration 855.
        stream.Write([0xAE, 0x06]); // Pulse duration 1710.

        // DATA block.
        stream.Write("DATA"u8);
        // DataHeader reads 12 bytes: size (4) + fields (8).
        // SizeOfHeaderExcludingTagAndSizeField = 8.
        // BlockLength = size - 8.
        // Data layout: zero pulse seq (numZero * 2 bytes) + one pulse seq (numOne * 2 bytes) + data bytes.
        // numZero = 1 (2 bytes), numOne = 1 (2 bytes), data = 2 bytes (16 bits). Total = 6 bytes.
        // size = 8 + 6 = 14.
        stream.Write([0x0E, 0x00, 0x00, 0x00]); // Size = 14.
        stream.Write([0x10, 0x00, 0x00, 0x00]); // SizeInBits = 16, InitialPulseLevel = false.
        stream.Write([0x00, 0x00]); // Tail = 0.
        stream.WriteByte(0x01); // NumberOfPulseInZeroBitSequence = 1.
        stream.WriteByte(0x01); // NumberOfPulseInOneBitSequence = 1.
        stream.Write([0x57, 0x03]); // Zero bit pulse = 855.
        stream.Write([0xAE, 0x06]); // One bit pulse = 1710.
        stream.Write([0xAA, 0x55]); // Data bytes.

        // PAUS Pause block.
        stream.Write("PAUS"u8);
        // PauseHeader reads 8 bytes: size (4) + duration (4).
        // SizeOfHeaderExcludingTagAndSizeField = 4.
        // BlockLength = size - 4. For no extra data: size = 4.
        stream.Write([0x04, 0x00, 0x00, 0x00]); // Size = 4.
        stream.Write([0xE8, 0x03, 0x00, 0x00]); // Duration = 1000, InitialPulseLevel = false.

        // BRWS Browse Point block.
        stream.Write("BRWS"u8);
        // BrowsePointHeader reads 4 bytes (just size).
        // SizeOfHeaderExcludingTagAndSizeField = 0.
        // BlockLength = size. For text "Hi": size = 2.
        stream.Write([0x02, 0x00, 0x00, 0x00]); // Size = 2.
        stream.Write("Hi"u8);

        // STOP block.
        stream.Write("STOP"u8);
        // StopHeader reads 6 bytes: size (4) + flags (2).
        // SizeOfHeaderExcludingTagAndSizeField = 2.
        // BlockLength = size - 2. For no extra data: size = 2.
        stream.Write([0x02, 0x00, 0x00, 0x00]); // Size = 2.
        stream.Write([0x01, 0x00]); // Only48k = true (word == 1).

        return stream.ToArray();
    }

    [Test]
    public void Read()
    {
        var data = BuildPzxData();
        using var stream = new MemoryStream(data);

        var file = PzxFormat.Instance.Read(stream);
        file.Format.Should().BeTheSameInstanceAs(PzxFormat.Instance);
        file.Blocks.Should().HaveCount(6);

        // PZXT Header.
        var header = file.Blocks[0].Should().BeOfType<PzxHeaderBlock>().Value;
        header.Header.Type.Should().Equal(PzxBlockType.Header);
        header.Header.MajorVersionNumber.Should().Equal(1);
        header.Header.MinorVersionNumber.Should().Equal(0);
        header.Info.Should().HaveCount(1);
        header.Info[0].Type.Should().Equal("Title");
        header.Info[0].Text.Should().Equal("Test");
        header.Info[0].ToString().Should().Equal("Title: Test");
        header.ToString().Should().Contain("PZX 1.0");
        header.ToString().Should().Contain("Title: Test");

        // PULS Pulse Sequence.
        var pulseSequence = file.Blocks[1].Should().BeOfType<PulseSequenceBlock>().Value;
        pulseSequence.Header.Type.Should().Equal(PzxBlockType.PulseSequence);
        pulseSequence.Pulses.Should().HaveCount(2);
        pulseSequence.Pulses[0].Count.Should().Equal(1);
        pulseSequence.Pulses[0].Duration.Should().Equal(855u);
        pulseSequence.Pulses[0].ToString().Should().Equal("1 x 855");
        pulseSequence.Pulses[1].Count.Should().Equal(1);
        pulseSequence.Pulses[1].Duration.Should().Equal(1710u);
        pulseSequence.ToString().Should().Contain("PulseSequence");

        // DATA.
        var dataBlock = file.Blocks[2].Should().BeOfType<DataBlock>().Value;
        dataBlock.Header.Type.Should().Equal(PzxBlockType.Data);
        dataBlock.Header.SizeInBits.Should().Equal(16u);
        dataBlock.Header.SizeInBytes.Should().Equal(2u);
        dataBlock.Header.ExtraBits.Should().Equal(0u);
        dataBlock.Header.InitialPulseLevel.Should().BeFalse();
        dataBlock.Header.Tail.Should().Equal(0);
        dataBlock.Header.NumberOfPulseInZeroBitSequence.Should().Equal(1);
        dataBlock.Header.NumberOfPulseInOneBitSequence.Should().Equal(1);
        dataBlock.ZeroBitPulseSequence.ToArray().Should().SequenceEqual(855);
        dataBlock.OneBitPulseSequence.ToArray().Should().SequenceEqual(1710);
        dataBlock.DataStream.ToArray().Should().SequenceEqual(0xAA, 0x55);
        dataBlock.DataStreamSize.Should().Equal(2);
        dataBlock.ToString().Should().Contain("Data:");

        // PAUS Pause.
        var pause = file.Blocks[3].Should().BeOfType<PauseBlock>().Value;
        pause.Header.Type.Should().Equal(PzxBlockType.Pause);
        pause.Header.Duration.Should().Equal(1000u);
        pause.Header.InitialPulseLevel.Should().BeFalse();
        pause.ToString().Should().Contain("Pause");
        pause.ToString().Should().Contain("1000");

        // BRWS Browse Point.
        var browsePoint = file.Blocks[4].Should().BeOfType<BrowsePointBlock>().Value;
        browsePoint.Header.Type.Should().Equal(PzxBlockType.BrowsePoint);
        browsePoint.Text.Should().Equal("Hi");
        browsePoint.ToString().Should().Contain("Hi");

        // STOP.
        var stop = file.Blocks[5].Should().BeOfType<StopBlock>().Value;
        stop.Header.Type.Should().Equal(PzxBlockType.Stop);
        stop.Header.Only48k.Should().BeTrue();
        stop.ToString().Should().Equal("Stop: 48k only");
    }

    [Test]
    public void Read_StopBlock_NotOnly48k()
    {
        using var stream = new MemoryStream();

        // Minimal PZX file: PZXT header + STOP with Only48k = false.
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]); // Size = 2.
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);

        stream.Write("STOP"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]); // Size = 2.
        stream.Write([0x00, 0x00]); // Only48k = false.

        stream.Position = 0;
        var file = PzxFormat.Instance.Read(stream);
        var stop = file.Blocks[1].Should().BeOfType<StopBlock>().Value;
        stop.Header.Only48k.Should().BeFalse();
        stop.ToString().Should().Equal("Stop: Always");
    }

    [Test]
    public void Read_PzxHeaderBlock_WithMultipleInfos()
    {
        using var stream = new MemoryStream();

        // PZXT with multiple info entries: Title=Test, Author=Joe.
        // Data: "Test\0Author\0Joe"
        var infoData = "Test\0Author\0Joe"u8;
        var size = 2 + infoData.Length;

        stream.Write("PZXT"u8);
        stream.Write([(byte)size, (byte)(size >> 8), 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        stream.Write(infoData);

        stream.Position = 0;
        var file = PzxFormat.Instance.Read(stream);
        var header = file.Blocks[0].Should().BeOfType<PzxHeaderBlock>().Value;
        header.Info.Should().HaveCount(2);
        header.Info[0].Type.Should().Equal("Title");
        header.Info[0].Text.Should().Equal("Test");
        header.Info[1].Type.Should().Equal("Author");
        header.Info[1].Text.Should().Equal("Joe");
    }

    [Test]
    public void Read_PzxHeaderBlock_NoInfo()
    {
        using var stream = new MemoryStream();

        // PZXT with no info data.
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]); // Size = 2 (just version, no data).
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);

        stream.Position = 0;
        var file = PzxFormat.Instance.Read(stream);
        var header = file.Blocks[0].Should().BeOfType<PzxHeaderBlock>().Value;
        header.Info.Should().BeEmpty();
        header.ToString().Should().Equal("PZX 1.0");
    }

    [Test]
    public void Read_PauseBlock_WithInitialPulseLevel()
    {
        using var stream = new MemoryStream();

        // PZXT header.
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);

        // PAUS with InitialPulseLevel = true: bit 31 set on the duration uint32.
        stream.Write("PAUS"u8);
        stream.Write([0x04, 0x00, 0x00, 0x00]); // Size = 4.
        stream.Write([0xE8, 0x03, 0x00, 0x80]); // Duration = 1000 with bit 31 set.

        stream.Position = 0;
        var file = PzxFormat.Instance.Read(stream);
        var pause = file.Blocks[1].Should().BeOfType<PauseBlock>().Value;
        pause.Header.Duration.Should().Equal(1000u);
        pause.Header.InitialPulseLevel.Should().BeTrue();
    }

    [Test]
    public void Read_DataBlock_WithExtraBits()
    {
        using var stream = new MemoryStream();

        // PZXT header.
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);

        // DATA block with SizeInBits = 11 (1 byte + 3 extra bits), InitialPulseLevel = true.
        // numZero = 0, numOne = 0, data = ceil(11/8) = 2 bytes.
        stream.Write("DATA"u8);
        var dataSize = 8 + 0 + 0 + 2; // 10
        stream.Write([(byte)dataSize, 0x00, 0x00, 0x00]);
        stream.Write([0x0B, 0x00, 0x00, 0x80]); // SizeInBits = 11, InitialPulseLevel = true (bit 31).
        stream.Write([0x00, 0x00]); // Tail = 0.
        stream.WriteByte(0x00); // NumZeroPulses = 0.
        stream.WriteByte(0x00); // NumOnePulses = 0.
        stream.Write([0xAA, 0x55]); // Data bytes.

        stream.Position = 0;
        var file = PzxFormat.Instance.Read(stream);
        var dataBlock = file.Blocks[1].Should().BeOfType<DataBlock>().Value;
        dataBlock.Header.SizeInBits.Should().Equal(11u);
        dataBlock.Header.SizeInBytes.Should().Equal(1u);
        dataBlock.Header.ExtraBits.Should().Equal(3u);
        dataBlock.Header.InitialPulseLevel.Should().BeTrue();
    }

    [Test]
    public void Read_ByteArray()
    {
        var data = BuildPzxData();
        var file = PzxFormat.Instance.Read(data);
        file.Blocks.Should().HaveCount(6);
    }

    [Test]
    public void Read_UnsupportedBlockType()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        stream.Write("ZZZZ"u8); // Unknown type.
        stream.Write([0x00, 0x00, 0x00, 0x00]);
        stream.Position = 0;

        AssertThat.Invoking(() => PzxFormat.Instance.Read(stream)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void PzxFile_TryLoadInto_ThrowsNotImplemented()
    {
        var data = BuildPzxData();
        using var stream = new MemoryStream(data);
        var file = PzxFormat.Instance.Read(stream);

        var memory = new byte[65536];
        AssertThat.Invoking(() => file.TryLoadInto(memory)).Should().Throw<NotImplementedException>();
    }

    [Test]
    public void Write_ThrowsNotImplemented()
    {
        var data = BuildPzxData();
        using var stream = new MemoryStream(data);
        var file = PzxFormat.Instance.Read(stream);

        using var output = new MemoryStream();
        AssertThat.Invoking(() => PzxFormat.Instance.Write(file, output)).Should().Throw<NotImplementedException>();
    }

    [Test]
    public void Write_ThrowsForWrongFileType()
    {
        var tapFile = TapFile.CreateCode("test", 0, [0xF3, 0xAF]);

        using var output = new MemoryStream();
        AssertThat.Invoking(() => PzxFormat.Instance.Write(tapFile, output)).Should().Throw<ArgumentException>();
    }

    [Explicit]
    [TestCaseSource(nameof(ReadTestCases))]
    public void CanRead([PathReference] string path)
    {
        using var file = File.OpenRead(path);

        // ReSharper disable once AccessToDisposedClosure
        PzxFormat.Instance.Invoking(t => t.Read(file)).Should().NotThrow();
    }

    [Pure]
    public static IEnumerable<TestCaseData> ReadTestCases()
    {
        foreach (var file in new DirectoryInfo("/home/mrkwatkins/ZX/").EnumerateFiles("*.pzx"))
        {
            yield return new TestCaseData(file.FullName).SetName(file.Name);
        }
    }
}