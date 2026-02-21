using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tzx;

[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class TzxFormatTests
{
    private static byte[] BuildTzxData()
    {
        using var stream = new MemoryStream();

        // TZX Header: "ZXTape!\x1A" + major 1 + minor 20.
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);

        // Standard Speed Data (0x10): pause=1000ms, data length=3, data=[0xAA, 0xBB, 0xCC].
        stream.WriteByte(0x10);
        stream.Write([0xE8, 0x03]); // Pause 1000ms LE.
        stream.Write([0x03, 0x00]); // Data length 3 LE.
        stream.Write([0xAA, 0xBB, 0xCC]);

        // Turbo Speed Data (0x11): 18 byte header + 2 bytes data.
        stream.WriteByte(0x11);
        stream.Write([0x78, 0x08]); // TStatesInPilotPulse = 2168.
        stream.Write([0x9B, 0x02]); // TStatesInSyncFirstPulse = 667.
        stream.Write([0xDF, 0x02]); // TStatesInSyncSecondPulse = 735.
        stream.Write([0x57, 0x03]); // TStatesInZeroBitPulse = 855.
        stream.Write([0xAE, 0x06]); // TStatesInOneBitPulse = 1710.
        stream.Write([0x7F, 0x1F]); // PulsesInPilotTone = 8063.
        stream.WriteByte(0x08); // UsedBitsInLastByte = 8.
        stream.Write([0xE8, 0x03]); // PauseAfterBlockMs = 1000.
        stream.Write([0x02, 0x00, 0x00]); // BlockLength = 2 (UInt24 LE).
        stream.Write([0xDD, 0xEE]);

        // Pure Tone (0x12): pulse length=2168, number of pulses=3223.
        stream.WriteByte(0x12);
        stream.Write([0x78, 0x08]); // LengthOfPulse = 2168.
        stream.Write([0x97, 0x0C]); // NumberOfPulses = 3223.

        // Pulse Sequence (0x13): 2 pulses.
        stream.WriteByte(0x13);
        stream.WriteByte(0x02); // NumberOfPulses = 2.
        stream.Write([0x9B, 0x02]); // Pulse 1 = 667.
        stream.Write([0xDF, 0x02]); // Pulse 2 = 735.

        // Pure Data (0x14): 10 byte header + 2 bytes data.
        stream.WriteByte(0x14);
        stream.Write([0x57, 0x03]); // TStatesInZeroBitPulse = 855.
        stream.Write([0xAE, 0x06]); // TStatesInOneBitPulse = 1710.
        stream.WriteByte(0x06); // UsedBitsInLastByte = 6.
        stream.Write([0xE8, 0x03]); // PauseAfterBlockMs = 1000.
        stream.Write([0x02, 0x00, 0x00]); // BlockLength = 2 (UInt24 LE).
        stream.Write([0xFF, 0xFE]);

        // Pause (0x20): 500ms.
        stream.WriteByte(0x20);
        stream.Write([0xF4, 0x01]); // PauseMs = 500.

        // Group Start (0x21): "Test".
        stream.WriteByte(0x21);
        stream.WriteByte(0x04); // Text length = 4.
        stream.Write("Test"u8);

        // Group End (0x22).
        stream.WriteByte(0x22);

        // Loop Start (0x24): 3 repetitions.
        stream.WriteByte(0x24);
        stream.Write([0x03, 0x00]); // NumberOfRepetitions = 3.

        // Loop End (0x25).
        stream.WriteByte(0x25);

        // Stop The Tape If 48K (0x2A): 4 bytes of zero.
        stream.WriteByte(0x2A);
        stream.Write([0x00, 0x00, 0x00, 0x00]);

        // Text Description (0x30): "Hello".
        stream.WriteByte(0x30);
        stream.WriteByte(0x05); // Text length = 5.
        stream.Write("Hello"u8);

        // Archive Info (0x32): 1 entry, FullTitle = "Monty".
        stream.WriteByte(0x32);
        stream.Write([0x08, 0x00]); // LengthOfWholeBlock = 8. (NumberOfTextStrings byte + entry data)
        stream.WriteByte(0x01); // NumberOfTextStrings = 1.
        // Entry: type=FullTitle(0x00), length=5, "Monty".
        stream.WriteByte(0x00);
        stream.WriteByte(0x05);
        stream.Write("Monty"u8);

        return stream.ToArray();
    }

    [Test]
    public void Read()
    {
        var data = BuildTzxData();
        using var stream = new MemoryStream(data);

        var file = TzxFormat.Instance.Read(stream);
        file.Format.Should().BeTheSameInstanceAs(TzxFormat.Instance);

        file.Header.MajorVersion.Should().Equal(1);
        file.Header.MinorVersion.Should().Equal(20);
        file.Header.IsValid.Should().BeTrue();

        file.Blocks.Should().HaveCount(13);

        // Standard Speed Data.
        var standardSpeed = file.Blocks[0].Should().BeOfType<StandardSpeedDataBlock>().Value;
        standardSpeed.Header.Type.Should().Equal(TzxBlockType.StandardSpeedData);
        standardSpeed.Header.PauseAfterBlockMs.Should().Equal(1000);
        standardSpeed.Header.PauseAfter.Should().Equal(TimeSpan.FromMilliseconds(1000));
        standardSpeed.Header.BlockLength.Should().Equal(3);
        standardSpeed.Length.Should().Equal(3);
        standardSpeed.Header.ToString().Should().Contain("StandardSpeedData");

        // Turbo Speed Data.
        var turboSpeed = file.Blocks[1].Should().BeOfType<TurboSpeedDataBlock>().Value;
        turboSpeed.Header.Type.Should().Equal(TzxBlockType.TurboSpeedData);
        turboSpeed.Header.TStatesInPilotPulse.Should().Equal(2168);
        turboSpeed.Header.TStatesInSyncFirstPulse.Should().Equal(667);
        turboSpeed.Header.TStatesInSyncSecondPulse.Should().Equal(735);
        turboSpeed.Header.TStatesInZeroBitPulse.Should().Equal(855);
        turboSpeed.Header.TStatesInOneBitPulse.Should().Equal(1710);
        turboSpeed.Header.PulsesInPilotTone.Should().Equal(8063);
        turboSpeed.Header.UsedBitsInLastByte.Should().Equal(8);
        turboSpeed.Header.PauseAfterBlockMs.Should().Equal(1000);
        turboSpeed.Header.PauseAfter.Should().Equal(TimeSpan.FromMilliseconds(1000));
        turboSpeed.Header.BlockLength.Should().Equal(2);
        turboSpeed.Length.Should().Equal(2);
        turboSpeed.Header.ToString().Should().Contain("TurboSpeedData");

        // Pure Tone.
        var pureTone = file.Blocks[2].Should().BeOfType<PureToneBlock>().Value;
        pureTone.Header.Type.Should().Equal(TzxBlockType.PureTone);
        pureTone.Header.LengthOfPulse.Should().Equal(2168);
        pureTone.Header.NumberOfPulses.Should().Equal(3223);
        pureTone.Header.BlockLength.Should().Equal(0);
        pureTone.Header.ToString().Should().Contain("PureTone");

        // Pulse Sequence.
        var pulseSequence = file.Blocks[3].Should().BeOfType<PulseSequenceBlock>().Value;
        pulseSequence.Header.Type.Should().Equal(TzxBlockType.PulseSequence);
        pulseSequence.Header.NumberOfPulses.Should().Equal(2);
        pulseSequence.Header.BlockLength.Should().Equal(4);
        pulseSequence.Pulses.ToArray().Should().SequenceEqual(new ushort[] { 667, 735 });
        pulseSequence.ToString().Should().Contain("PulseSequence");

        // Pure Data.
        var pureData = file.Blocks[4].Should().BeOfType<PureDataBlock>().Value;
        pureData.Header.Type.Should().Equal(TzxBlockType.PureData);
        pureData.Header.TStatesInZeroBitPulse.Should().Equal(855);
        pureData.Header.TStatesInOneBitPulse.Should().Equal(1710);
        pureData.Header.UsedBitsInLastByte.Should().Equal(6);
        pureData.Header.PauseAfterBlockMs.Should().Equal(1000);
        pureData.Header.PauseAfter.Should().Equal(TimeSpan.FromMilliseconds(1000));
        pureData.Header.BlockLength.Should().Equal(2);
        pureData.Header.ToString().Should().Contain("PureData");

        // Pause.
        var pause = file.Blocks[5].Should().BeOfType<PauseBlock>().Value;
        pause.Header.Type.Should().Equal(TzxBlockType.Pause);
        pause.Header.PauseMs.Should().Equal(500);
        pause.Header.Pause.Should().Equal(TimeSpan.FromMilliseconds(500));
        pause.Header.ToString().Should().Contain("Pause");

        // Group Start.
        var groupStart = file.Blocks[6].Should().BeOfType<GroupStartBlock>().Value;
        groupStart.Header.Type.Should().Equal(TzxBlockType.GroupStart);
        groupStart.Header.BlockLength.Should().Equal(4);
        groupStart.Text.Should().Equal("Test");
        groupStart.ToString().Should().Contain("Test");

        // Group End.
        var groupEnd = file.Blocks[7].Should().BeOfType<GroupEndBlock>().Value;
        groupEnd.Header.Type.Should().Equal(TzxBlockType.GroupEnd);
        groupEnd.Header.BlockLength.Should().Equal(0);
        groupEnd.ToString().Should().Equal("GroupEnd");

        // Loop Start.
        var loopStart = file.Blocks[8].Should().BeOfType<LoopStartBlock>().Value;
        loopStart.Header.Type.Should().Equal(TzxBlockType.LoopStart);
        loopStart.Header.NumberOfRepetitions.Should().Equal(3);
        loopStart.Header.ToString().Should().Contain("3 repetitions");

        // Loop End.
        var loopEnd = file.Blocks[9].Should().BeOfType<LoopEndBlock>().Value;
        loopEnd.Header.Type.Should().Equal(TzxBlockType.LoopEnd);
        loopEnd.Header.BlockLength.Should().Equal(0);
        loopEnd.ToString().Should().Equal("LoopEnd");

        // Stop The Tape If 48K.
        // ReSharper disable once InconsistentNaming
        var stop48k = file.Blocks[10].Should().BeOfType<StopTheTapeIf48KBlock>().Value;
        stop48k.Header.Type.Should().Equal(TzxBlockType.StopTheTapeIf48K);
        stop48k.ToString().Should().Equal("StopTheTapeIf48K");

        // Text Description.
        var textDescription = file.Blocks[11].Should().BeOfType<TextDescriptionBlock>().Value;
        textDescription.Header.Type.Should().Equal(TzxBlockType.TextDescription);
        textDescription.Header.BlockLength.Should().Equal(5);
        textDescription.Text.Should().Equal("Hello");
        textDescription.ToString().Should().Contain("Hello");

        // Archive Info.
        var archiveInfo = file.Blocks[12].Should().BeOfType<ArchiveInfoBlock>().Value;
        archiveInfo.Header.Type.Should().Equal(TzxBlockType.ArchiveInfo);
        archiveInfo.Header.LengthOfWholeBlock.Should().Equal(8);
        archiveInfo.Header.NumberOfTextStrings.Should().Equal(1);
        archiveInfo.Header.BlockLength.Should().Equal(7);
        archiveInfo.Header.ToString().Should().Contain("1 entries");
        archiveInfo.Entries.Should().HaveCount(1);
        archiveInfo.Entries[0].Type.Should().Equal(ArchiveInfoType.FullTitle);
        archiveInfo.Entries[0].Text.Should().Equal("Monty");
        archiveInfo.Entries[0].ToString().Should().Contain("Monty");
    }

    [Test]
    public void Read_ByteArray()
    {
        var data = BuildTzxData();
        var file = TzxFormat.Instance.Read(data);
        file.Blocks.Should().HaveCount(13);
    }

    [Test]
    public void Read_InvalidHeader()
    {
        var data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x14 };
        using var stream = new MemoryStream(data);

        AssertThat.Invoking(() => TzxFormat.Instance.Read(stream)).Should().Throw<IOException>()
            .Exception.Message.Should().Equal("Not a valid TZX file.");
    }

    [Test]
    public void Read_UnsupportedBlockType()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        stream.WriteByte(0x99); // Unknown type.
        stream.Position = 0;

        AssertThat.Invoking(() => TzxFormat.Instance.Read(stream)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void TzxHeader_IsValid_False()
    {
        var header = new TzxHeader([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x14]);
        header.IsValid.Should().BeFalse();
    }

    [Test]
    public void TzxHeader_Constructor_MajorMinor()
    {
        var header = new TzxHeader(1, 20);
        header.MajorVersion.Should().Equal(1);
        header.MinorVersion.Should().Equal(20);
        header.IsValid.Should().BeTrue();
    }

    [Test]
    public void TzxFile_TryLoadInto_ThrowsNotImplemented()
    {
        var data = BuildTzxData();
        using var stream = new MemoryStream(data);
        var file = TzxFormat.Instance.Read(stream);

        var memory = new byte[65536];
        AssertThat.Invoking(() => file.TryLoadInto(memory)).Should().Throw<NotImplementedException>();
    }

    [Test]
    public void Write_RoundTrips()
    {
        var data = BuildTzxData();
        using var readStream = new MemoryStream(data);
        var file = TzxFormat.Instance.Read(readStream);

        using var writeStream = new MemoryStream();
        TzxFormat.Instance.Write(file, writeStream);

        writeStream.ToArray().Should().SequenceEqual(data);
    }

    [Test]
    public void Write_ThrowsForWrongFileType()
    {
        var tapFile = TapFile.CreateCode("test", 0, [0xF3, 0xAF]);

        using var output = new MemoryStream();
        AssertThat.Invoking(() => TzxFormat.Instance.Write(tapFile, output)).Should().Throw<ArgumentException>();
    }

    [Explicit]
    [TestCaseSource(nameof(ReadTestCases))]
    public void CanRead([PathReference] string path)
    {
        using var file = File.OpenRead(path);

        var tzx = TzxFormat.Instance.Read(file);

        foreach (var block in tzx.Blocks)
        {
            TestContext.Out.WriteLine(block);
        }
    }

    [Pure]
    public static IEnumerable<TestCaseData> ReadTestCases()
    {
        foreach (var file in new DirectoryInfo("/home/mrkwatkins/ZX/").EnumerateFiles("*.tzx"))
        {
            yield return new TestCaseData(file.FullName).SetName(file.Name);
        }
    }
}