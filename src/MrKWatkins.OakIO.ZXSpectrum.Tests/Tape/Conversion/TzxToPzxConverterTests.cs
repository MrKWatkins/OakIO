using MrKWatkins.OakIO.ZXSpectrum.Tape.Conversion;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;
using PzxDataBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.DataBlock;
using PzxPauseBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.PauseBlock;
using PzxPulseSequenceBlock = MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx.PulseSequenceBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Conversion;

public sealed class TzxToPzxConverterTests
{
    private static TzxFile ReadTzx(byte[] data)
    {
        using var stream = new MemoryStream(data);
        return TzxFormat.Instance.Read(stream);
    }

    private static byte[] BuildMinimalTzxHeader()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        return stream.ToArray();
    }

    [Test]
    public void Convert_ReturnsValidPzxHeader()
    {
        var tzx = ReadTzx(BuildMinimalTzxHeader());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        pzx.Blocks.Should().HaveCount(1);
        var header = pzx.Blocks[0].Should().BeOfType<PzxHeaderBlock>().Value;
        header.Header.MajorVersionNumber.Should().Equal(1);
        header.Header.MinorVersionNumber.Should().Equal(0);
        header.Info.Should().BeEmpty();
    }

    [Test]
    public void Convert_PureToneBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pure Tone: 5 pulses of 2168 T-states.
        stream.WriteByte(0x12);
        stream.Write([0x78, 0x08]); // LengthOfPulse = 2168.
        stream.Write([0x05, 0x00]); // NumberOfPulses = 5.

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // Expect: PZXT header + PULS block.
        pzx.Blocks.Should().HaveCount(2);
        var pulseBlock = pzx.Blocks[1].Should().BeOfType<PzxPulseSequenceBlock>().Value;
        pulseBlock.Pulses.Should().HaveCount(1);
        pulseBlock.Pulses[0].Count.Should().Equal(5);
        pulseBlock.Pulses[0].Duration.Should().Equal(2168u);
    }

    [Test]
    public void Convert_PulseSequenceBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pulse Sequence: 2 pulses.
        stream.WriteByte(0x13);
        stream.WriteByte(0x02);
        stream.Write([0x9B, 0x02]); // 667.
        stream.Write([0xDF, 0x02]); // 735.

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        pzx.Blocks.Should().HaveCount(2);
        var pulseBlock = pzx.Blocks[1].Should().BeOfType<PzxPulseSequenceBlock>().Value;
        pulseBlock.Pulses.Should().HaveCount(2);
        pulseBlock.Pulses[0].Count.Should().Equal(1);
        pulseBlock.Pulses[0].Duration.Should().Equal(667u);
        pulseBlock.Pulses[1].Count.Should().Equal(1);
        pulseBlock.Pulses[1].Duration.Should().Equal(735u);
    }

    [Test]
    public void Convert_PureDataBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pure Data: 2 bytes, all 8 bits used in last byte.
        stream.WriteByte(0x14);
        stream.Write([0x57, 0x03]); // TStatesInZeroBitPulse = 855.
        stream.Write([0xAE, 0x06]); // TStatesInOneBitPulse = 1710.
        stream.WriteByte(0x08);     // UsedBitsInLastByte = 8.
        stream.Write([0xE8, 0x03]); // PauseAfterBlockMs = 1000.
        stream.Write([0x02, 0x00, 0x00]); // BlockLength = 2.
        stream.Write([0xAA, 0x55]);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // Expect: PZXT header + DATA block + PAUS block.
        pzx.Blocks.Should().HaveCount(3);

        var dataBlock = pzx.Blocks[1].Should().BeOfType<PzxDataBlock>().Value;
        dataBlock.Header.SizeInBits.Should().Equal(16u);
        dataBlock.Header.NumberOfPulseInZeroBitSequence.Should().Equal(2);
        dataBlock.Header.NumberOfPulseInOneBitSequence.Should().Equal(2);
        dataBlock.ZeroBitPulseSequence.ToArray().Should().SequenceEqual(new ushort[] { 855, 855 });
        dataBlock.OneBitPulseSequence.ToArray().Should().SequenceEqual(new ushort[] { 1710, 1710 });
        dataBlock.Header.Tail.Should().Equal(0);
        dataBlock.DataStream.ToArray().Should().SequenceEqual(0xAA, 0x55);

        var pause = pzx.Blocks[2].Should().BeOfType<PzxPauseBlock>().Value;
        pause.Header.Duration.Should().Equal(3500000u); // 1000ms * 3500.
    }

    [Test]
    public void Convert_PureDataBlock_WithExtraBits()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pure Data: 2 bytes, 5 bits used in last byte → 13 total bits.
        stream.WriteByte(0x14);
        stream.Write([0x57, 0x03]); // 855.
        stream.Write([0xAE, 0x06]); // 1710.
        stream.WriteByte(0x05);     // UsedBitsInLastByte = 5.
        stream.Write([0xE8, 0x03]); // PauseAfterBlockMs = 1000.
        stream.Write([0x02, 0x00, 0x00]); // BlockLength = 2.
        stream.Write([0xAA, 0x55]);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        var dataBlock = pzx.Blocks[1].Should().BeOfType<PzxDataBlock>().Value;
        dataBlock.Header.SizeInBits.Should().Equal(13u); // 8 + 5.
    }

    [Test]
    public void Convert_PureDataBlock_NoPause()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pure Data: 1 byte, no pause.
        stream.WriteByte(0x14);
        stream.Write([0x57, 0x03]); // 855.
        stream.Write([0xAE, 0x06]); // 1710.
        stream.WriteByte(0x08);     // 8 bits.
        stream.Write([0x00, 0x00]); // PauseAfterBlockMs = 0.
        stream.Write([0x01, 0x00, 0x00]); // BlockLength = 1.
        stream.WriteByte(0xFF);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // Expect: PZXT header + DATA block (no PAUS since pause = 0).
        pzx.Blocks.Should().HaveCount(2);
        var dataBlock = pzx.Blocks[1].Should().BeOfType<PzxDataBlock>().Value;
        dataBlock.Header.Tail.Should().Equal(0);
    }

    [Test]
    public void Convert_PureDataBlock_BitsInLastByteZero_TreatedAs8()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pure Data: 2 bytes, UsedBitsInLastByte = 0 (means 8 per TZX spec).
        stream.WriteByte(0x14);
        stream.Write([0x57, 0x03]); // 855.
        stream.Write([0xAE, 0x06]); // 1710.
        stream.WriteByte(0x00);     // UsedBitsInLastByte = 0 (means 8).
        stream.Write([0x00, 0x00]); // PauseAfterBlockMs = 0.
        stream.Write([0x02, 0x00, 0x00]); // BlockLength = 2.
        stream.Write([0xAA, 0x55]);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        pzx.Blocks.Should().HaveCount(2);
        var dataBlock = pzx.Blocks[1].Should().BeOfType<PzxDataBlock>().Value;
        dataBlock.Header.SizeInBits.Should().Equal(16u);
        dataBlock.DataStream.ToArray().Should().SequenceEqual(0xAA, 0x55);
    }

    [Test]
    public void Convert_PauseBlock_NonZero()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pause: 500ms.
        stream.WriteByte(0x20);
        stream.Write([0xF4, 0x01]); // 500ms.

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // Expect: PZXT header + PAUS block.
        pzx.Blocks.Should().HaveCount(2);
        var pause = pzx.Blocks[1].Should().BeOfType<PzxPauseBlock>().Value;
        pause.Header.Duration.Should().Equal(1750000u); // 500 * 3500.
        pause.Header.InitialPulseLevel.Should().BeFalse();
    }

    [Test]
    public void Convert_PauseBlock_Zero_ConvertsToStop()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pause: 0ms → stop.
        stream.WriteByte(0x20);
        stream.Write([0x00, 0x00]);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        pzx.Blocks.Should().HaveCount(2);
        var stop = pzx.Blocks[1].Should().BeOfType<StopBlock>().Value;
        stop.Header.Only48k.Should().BeFalse();
    }

    [Test]
    public void Convert_StopTheTapeIf48KBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Stop If 48K.
        stream.WriteByte(0x2A);
        stream.Write([0x00, 0x00, 0x00, 0x00]);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        pzx.Blocks.Should().HaveCount(2);
        var stop = pzx.Blocks[1].Should().BeOfType<StopBlock>().Value;
        stop.Header.Only48k.Should().BeTrue();
    }

    [Test]
    public void Convert_GroupStartBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Group Start: "Test".
        stream.WriteByte(0x21);
        stream.WriteByte(0x04);
        stream.Write("Test"u8);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        pzx.Blocks.Should().HaveCount(2);
        var browse = pzx.Blocks[1].Should().BeOfType<BrowsePointBlock>().Value;
        browse.Text.Should().Equal("Test");
    }

    [Test]
    public void Convert_GroupEndBlock_Ignored()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Group End.
        stream.WriteByte(0x22);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // Only the PZXT header block.
        pzx.Blocks.Should().HaveCount(1);
    }

    [Test]
    public void Convert_TextDescriptionBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Text Description: "Hello".
        stream.WriteByte(0x30);
        stream.WriteByte(0x05);
        stream.Write("Hello"u8);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        pzx.Blocks.Should().HaveCount(2);
        var browse = pzx.Blocks[1].Should().BeOfType<BrowsePointBlock>().Value;
        browse.Text.Should().Equal("Hello");
    }

    [Test]
    public void Convert_ArchiveInfoBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Archive Info: 2 entries - FullTitle="Monty", Authors="Gremlin".
        stream.WriteByte(0x32);
        stream.Write([0x11, 0x00]); // LengthOfWholeBlock = 17 (1 + 1+1+5 + 1+1+7).
        stream.WriteByte(0x02);     // NumberOfTextStrings = 2.
        // Entry 1: FullTitle.
        stream.WriteByte(0x00);
        stream.WriteByte(0x05);
        stream.Write("Monty"u8);
        // Entry 2: Authors.
        stream.WriteByte(0x02);
        stream.WriteByte(0x07);
        stream.Write("Gremlin"u8);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        var header = pzx.Blocks[0].Should().BeOfType<PzxHeaderBlock>().Value;
        header.Info.Should().HaveCount(2);
        header.Info[0].Type.Should().Equal("Title");
        header.Info[0].Text.Should().Equal("Monty");
        header.Info[1].Type.Should().Equal("Author");
        header.Info[1].Text.Should().Equal("Gremlin");
    }

    [Test]
    public void Convert_ArchiveInfoBlock_NoTitle_UsesDefault()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Archive Info: 1 entry - Authors only.
        stream.WriteByte(0x32);
        stream.Write([0x0A, 0x00]); // LengthOfWholeBlock = 10.
        stream.WriteByte(0x01);     // NumberOfTextStrings = 1.
        stream.WriteByte(0x02);     // Authors.
        stream.WriteByte(0x07);
        stream.Write("Gremlin"u8);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        var header = pzx.Blocks[0].Should().BeOfType<PzxHeaderBlock>().Value;
        header.Info.Should().HaveCount(2);
        header.Info[0].Type.Should().Equal("Title");
        header.Info[0].Text.Should().Equal("Some tape");
        header.Info[1].Type.Should().Equal("Author");
        header.Info[1].Text.Should().Equal("Gremlin");
    }

    [Test]
    public void Convert_StandardSpeedDataBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Standard Speed Data: pause=1000ms, data=[0x00, 0xFF] (flag byte 0x00 < 128 → long leader).
        stream.WriteByte(0x10);
        stream.Write([0xE8, 0x03]); // Pause 1000ms.
        stream.Write([0x02, 0x00]); // Data length 2.
        stream.Write([0x00, 0xFF]);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // Expect: PZXT header + PULS (pilot) + DATA + PAUS.
        pzx.Blocks.Should().HaveCount(4);

        // Pilot pulses: 8063 × 2168, 1 × 667, 1 × 735.
        var pilotBlock = pzx.Blocks[1].Should().BeOfType<PzxPulseSequenceBlock>().Value;
        pilotBlock.Pulses.Should().HaveCount(3);
        pilotBlock.Pulses[0].Count.Should().Equal(8063);
        pilotBlock.Pulses[0].Duration.Should().Equal(2168u);
        pilotBlock.Pulses[1].Count.Should().Equal(1);
        pilotBlock.Pulses[1].Duration.Should().Equal(667u);
        pilotBlock.Pulses[2].Count.Should().Equal(1);
        pilotBlock.Pulses[2].Duration.Should().Equal(735u);

        // Data block: 16 bits, 2-pulse sequences.
        var dataBlock = pzx.Blocks[2].Should().BeOfType<PzxDataBlock>().Value;
        dataBlock.Header.SizeInBits.Should().Equal(16u);
        dataBlock.Header.NumberOfPulseInZeroBitSequence.Should().Equal(2);
        dataBlock.Header.NumberOfPulseInOneBitSequence.Should().Equal(2);
        dataBlock.ZeroBitPulseSequence.ToArray().Should().SequenceEqual(new ushort[] { 855, 855 });
        dataBlock.OneBitPulseSequence.ToArray().Should().SequenceEqual(new ushort[] { 1710, 1710 });
        dataBlock.Header.Tail.Should().Equal(945);
        dataBlock.DataStream.ToArray().Should().SequenceEqual(0x00, 0xFF);

        // Pause: 1000ms * 3500 = 3500000 T-states.
        var pause = pzx.Blocks[3].Should().BeOfType<PzxPauseBlock>().Value;
        pause.Header.Duration.Should().Equal(3500000u);
    }

    [Test]
    public void Convert_StandardSpeedDataBlock_ShortLeader()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Standard Speed Data: flag byte 0xFF >= 128 → short leader (3223).
        stream.WriteByte(0x10);
        stream.Write([0xE8, 0x03]); // Pause 1000ms.
        stream.Write([0x01, 0x00]); // Data length 1.
        stream.WriteByte(0xFF);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        var pilotBlock = pzx.Blocks[1].Should().BeOfType<PzxPulseSequenceBlock>().Value;
        pilotBlock.Pulses[0].Count.Should().Equal(3223);
        pilotBlock.Pulses[0].Duration.Should().Equal(2168u);
    }

    [Test]
    public void Convert_TurboSpeedDataBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Turbo Speed Data.
        stream.WriteByte(0x11);
        stream.Write([0x78, 0x08]); // TStatesInPilotPulse = 2168.
        stream.Write([0x9B, 0x02]); // TStatesInSyncFirstPulse = 667.
        stream.Write([0xDF, 0x02]); // TStatesInSyncSecondPulse = 735.
        stream.Write([0x57, 0x03]); // TStatesInZeroBitPulse = 855.
        stream.Write([0xAE, 0x06]); // TStatesInOneBitPulse = 1710.
        stream.Write([0x05, 0x00]); // PulsesInPilotTone = 5.
        stream.WriteByte(0x08);     // UsedBitsInLastByte = 8.
        stream.Write([0xE8, 0x03]); // PauseAfterBlockMs = 1000.
        stream.Write([0x01, 0x00, 0x00]); // BlockLength = 1.
        stream.WriteByte(0xAA);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // PZXT header + PULS (pilot) + DATA + PAUS.
        pzx.Blocks.Should().HaveCount(4);

        var pilotBlock = pzx.Blocks[1].Should().BeOfType<PzxPulseSequenceBlock>().Value;
        pilotBlock.Pulses.Should().HaveCount(3);
        pilotBlock.Pulses[0].Count.Should().Equal(5);
        pilotBlock.Pulses[0].Duration.Should().Equal(2168u);
        pilotBlock.Pulses[1].Count.Should().Equal(1);
        pilotBlock.Pulses[1].Duration.Should().Equal(667u);
        pilotBlock.Pulses[2].Count.Should().Equal(1);
        pilotBlock.Pulses[2].Duration.Should().Equal(735u);

        var dataBlock = pzx.Blocks[2].Should().BeOfType<PzxDataBlock>().Value;
        dataBlock.Header.SizeInBits.Should().Equal(8u);
        dataBlock.ZeroBitPulseSequence.ToArray().Should().SequenceEqual(new ushort[] { 855, 855 });
        dataBlock.OneBitPulseSequence.ToArray().Should().SequenceEqual(new ushort[] { 1710, 1710 });
        dataBlock.Header.Tail.Should().Equal(0);
        dataBlock.DataStream.ToArray().Should().SequenceEqual(0xAA);
    }

    [Test]
    public void Convert_LoopBlocks()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Loop Start: 3 repetitions.
        stream.WriteByte(0x24);
        stream.Write([0x03, 0x00]);

        // Group Start inside loop: "L".
        stream.WriteByte(0x21);
        stream.WriteByte(0x01);
        stream.Write("L"u8);

        // Loop End.
        stream.WriteByte(0x25);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // PZXT header + 3 × BRWS blocks.
        pzx.Blocks.Should().HaveCount(4);
        pzx.Blocks[1].Should().BeOfType<BrowsePointBlock>().Value.Text.Should().Equal("L");
        pzx.Blocks[2].Should().BeOfType<BrowsePointBlock>().Value.Text.Should().Equal("L");
        pzx.Blocks[3].Should().BeOfType<BrowsePointBlock>().Value.Text.Should().Equal("L");
    }

    [Test]
    public void Convert_AllBlockTypes()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Archive Info: title "Monty".
        stream.WriteByte(0x32);
        stream.Write([0x08, 0x00]); // LengthOfWholeBlock = 8.
        stream.WriteByte(0x01);     // NumberOfTextStrings = 1.
        stream.WriteByte(0x00);
        stream.WriteByte(0x05);
        stream.Write("Monty"u8);

        // Pure Tone: 3 × 2168.
        stream.WriteByte(0x12);
        stream.Write([0x78, 0x08]);
        stream.Write([0x03, 0x00]);

        // Pulse Sequence: 2 pulses [667, 735].
        stream.WriteByte(0x13);
        stream.WriteByte(0x02);
        stream.Write([0x9B, 0x02]);
        stream.Write([0xDF, 0x02]);

        // Pure Data: 1 byte, no pause.
        stream.WriteByte(0x14);
        stream.Write([0x57, 0x03]);
        stream.Write([0xAE, 0x06]);
        stream.WriteByte(0x08);
        stream.Write([0x00, 0x00]);
        stream.Write([0x01, 0x00, 0x00]);
        stream.WriteByte(0xAA);

        // Group Start: "G".
        stream.WriteByte(0x21);
        stream.WriteByte(0x01);
        stream.Write("G"u8);

        // Group End.
        stream.WriteByte(0x22);

        // Pause: 100ms.
        stream.WriteByte(0x20);
        stream.Write([0x64, 0x00]);

        // Text Description: "Hi".
        stream.WriteByte(0x30);
        stream.WriteByte(0x02);
        stream.Write("Hi"u8);

        // Stop If 48K.
        stream.WriteByte(0x2A);
        stream.Write([0x00, 0x00, 0x00, 0x00]);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // Verify all block types are present.
        // PZXT header, PULS (tone+sequence combined), DATA, BRWS(G), PULS(level-change), PAUS, BRWS(Hi), STOP.
        pzx.Blocks[0].Should().BeOfType<PzxHeaderBlock>().Value.Info[0].Text.Should().Equal("Monty");

        // The pure tone and pulse sequence pulses are accumulated and flushed when the DATA block is emitted.
        var pulseBlock = pzx.Blocks[1].Should().BeOfType<PzxPulseSequenceBlock>().Value;
        pulseBlock.Pulses.Should().HaveCount(3);
        pulseBlock.Pulses[0].Count.Should().Equal(3);
        pulseBlock.Pulses[0].Duration.Should().Equal(2168u);
        pulseBlock.Pulses[1].Count.Should().Equal(1);
        pulseBlock.Pulses[1].Duration.Should().Equal(667u);
        pulseBlock.Pulses[2].Count.Should().Equal(1);
        pulseBlock.Pulses[2].Duration.Should().Equal(735u);

        pzx.Blocks[2].Should().BeOfType<PzxDataBlock>();
        pzx.Blocks[3].Should().BeOfType<BrowsePointBlock>().Value.Text.Should().Equal("G");

        // When level is high and a pause is needed, a 1ms transition pulse is emitted.
        pzx.Blocks[4].Should().BeOfType<PzxPulseSequenceBlock>();
        pzx.Blocks[5].Should().BeOfType<PzxPauseBlock>();
        pzx.Blocks[6].Should().BeOfType<BrowsePointBlock>().Value.Text.Should().Equal("Hi");
        pzx.Blocks[7].Should().BeOfType<StopBlock>().Value.Header.Only48k.Should().BeTrue();
    }

    [Test]
    public void Convert_PauseBlock_WithHighLevel_EmitsExtraPulse()
    {
        // When the current level is high (from prior pulses), a TZX pause should
        // emit a 1ms pulse to bring level low before the pause.
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pure Tone: 1 pulse (leaves level high after).
        stream.WriteByte(0x12);
        stream.Write([0x78, 0x08]); // 2168.
        stream.Write([0x01, 0x00]); // 1 pulse.

        // Pause: 100ms.
        stream.WriteByte(0x20);
        stream.Write([0x64, 0x00]);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // PZXT header + PULS (tone pulse + 1ms transition pulse) + PAUS.
        pzx.Blocks.Should().HaveCount(3);

        var pulseBlock = pzx.Blocks[1].Should().BeOfType<PzxPulseSequenceBlock>().Value;
        // After 1 pure tone pulse (alternating), the level is high.
        // RenderPause detects high level and emits an extra pulse of MillisecondCycles.
        // So we get: 1×2168, 1×3500.
        pulseBlock.Pulses.Should().HaveCount(2);
        pulseBlock.Pulses[0].Duration.Should().Equal(2168u);
        pulseBlock.Pulses[1].Duration.Should().Equal(3500u);

        var pause = pzx.Blocks[2].Should().BeOfType<PzxPauseBlock>().Value;
        pause.Header.Duration.Should().Equal(350000u); // 100 * 3500.
    }

    [Test]
    public void Convert_PureDataBlock_Pause1ms_EmitsPauseBlock()
    {
        using var stream = new MemoryStream();
        stream.Write(BuildMinimalTzxHeader());

        // Pure Data: 1 byte, pause = 1ms. Always emits a separate PAUS block.
        stream.WriteByte(0x14);
        stream.Write([0x57, 0x03]); // 855.
        stream.Write([0xAE, 0x06]); // 1710.
        stream.WriteByte(0x08);     // 8 bits.
        stream.Write([0x01, 0x00]); // PauseAfterBlockMs = 1.
        stream.Write([0x01, 0x00, 0x00]); // BlockLength = 1.
        stream.WriteByte(0xAA);

        var tzx = ReadTzx(stream.ToArray());
        var converter = new TzxToPzxConverter();

        var pzx = converter.Convert(tzx);

        // Expect: PZXT header + DATA (no tail for PureData) + PAUS.
        pzx.Blocks.Should().HaveCount(3);

        var dataBlock = pzx.Blocks[1].Should().BeOfType<PzxDataBlock>().Value;
        dataBlock.Header.Tail.Should().Equal(0);

        var pause = pzx.Blocks[2].Should().BeOfType<PzxPauseBlock>().Value;
        pause.Header.Duration.Should().Equal(3500u); // 1ms * 3500.
    }
}