using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tzx;

public sealed class TzxToTapeConverterTests
{
    [Test]
    public void Convert_ReturnsBlocksForBlocks()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = new TapToTzxConverter().Convert(tap);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_PureToneBlock()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        stream.WriteByte(0x12);
        stream.Write([0x78, 0x08]);
        stream.Write([0x0A, 0x00]);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_PauseBlock()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        stream.WriteByte(0x20);
        stream.Write([0xE8, 0x03]);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_TurboSpeedDataBlock()
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        stream.WriteByte(0x11);
        // TurboSpeedDataHeader: 18 bytes.
        // TStatesInPilotPulse, SyncFirst, SyncSecond, ZeroBit, OneBit, PulsesInPilotTone
        stream.Write([0x78, 0x08]); // pilot: 2168
        stream.Write([0x9B, 0x02]); // sync1: 667
        stream.Write([0xDF, 0x02]); // sync2: 735
        stream.Write([0x57, 0x03]); // zero bit: 855
        stream.Write([0xAE, 0x06]); // one bit: 1710
        stream.Write([0x7F, 0x1F]); // pilot pulses: 8063
        stream.WriteByte(0x08);     // used bits: 8 (non-zero, so no default)
        stream.Write([0x00, 0x00]); // pause after: 0 ms (no pause block yielded)
        stream.Write([0x01, 0x00, 0x00]); // data length: 1
        stream.WriteByte(0xFF);     // 1 byte of data
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_TurboSpeedDataBlock_UsedBitsZero()
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        stream.WriteByte(0x11);
        stream.Write([0x78, 0x08]); // pilot: 2168
        stream.Write([0x9B, 0x02]); // sync1: 667
        stream.Write([0xDF, 0x02]); // sync2: 735
        stream.Write([0x57, 0x03]); // zero bit: 855
        stream.Write([0xAE, 0x06]); // one bit: 1710
        stream.Write([0x7F, 0x1F]); // pilot pulses: 8063
        stream.WriteByte(0x00);     // used bits: 0 (defaults to 8)
        stream.Write([0x64, 0x00]); // pause after: 100 ms (pause block yielded)
        stream.Write([0x01, 0x00, 0x00]); // data length: 1
        stream.WriteByte(0xFF);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_PulseSequenceBlock()
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        stream.WriteByte(0x13);
        stream.WriteByte(0x01);     // 1 pulse
        stream.Write([0xE8, 0x03]); // 1000 T-states
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_PureDataBlock()
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        stream.WriteByte(0x14);
        // PureDataHeader: 10 bytes.
        stream.Write([0x57, 0x03]); // zero bit: 855
        stream.Write([0xAE, 0x06]); // one bit: 1710
        stream.WriteByte(0x08);     // used bits: 8 (non-zero)
        stream.Write([0x00, 0x00]); // pause after: 0 ms (no pause block)
        stream.Write([0x01, 0x00, 0x00]); // data length: 1
        stream.WriteByte(0xFF);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_PureDataBlock_UsedBitsZeroWithPause()
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        stream.WriteByte(0x14);
        stream.Write([0x57, 0x03]); // zero bit: 855
        stream.Write([0xAE, 0x06]); // one bit: 1710
        stream.WriteByte(0x00);     // used bits: 0 (defaults to 8)
        stream.Write([0x64, 0x00]); // pause after: 100 ms (pause block yielded)
        stream.Write([0x01, 0x00, 0x00]); // data length: 1
        stream.WriteByte(0xFF);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_LoopWithRepetitions()
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        // LoopStart with 2 repetitions
        stream.WriteByte(0x24);
        stream.Write([0x02, 0x00]);
        // PureTone inside loop
        stream.WriteByte(0x12);
        stream.Write([0x78, 0x08]); // 2168 T-states per pulse
        stream.Write([0x0A, 0x00]); // 10 pulses
        // LoopEnd
        stream.WriteByte(0x25);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        // A TapeLoopBlock should have been created (loopCount - 1 repetitions).
        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_LoopWithZeroRepetitions()
    {
        using var stream = new MemoryStream();
        WriteHeader(stream);
        // LoopStart with 0 repetitions — blocks added inline.
        stream.WriteByte(0x24);
        stream.Write([0x00, 0x00]);
        // PureTone inside loop
        stream.WriteByte(0x12);
        stream.Write([0x78, 0x08]); // 2168 T-states per pulse
        stream.Write([0x0A, 0x00]); // 10 pulses
        // LoopEnd
        stream.WriteByte(0x25);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_StandardSpeedDataBlock_EmptyData()
    {
        // Covers the "standardSpeed.Length > 0" false branch (flagByte defaults to 0xFF).
        using var stream = new MemoryStream();
        WriteHeader(stream);
        stream.WriteByte(0x10);     // StandardSpeedData
        stream.Write([0x00, 0x00]); // PauseAfterBlockMs = 0
        stream.Write([0x00, 0x00]); // BlockLength = 0 (no data)
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var tape = new TzxToTapeConverter().Convert(tzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    private static void WriteHeader(MemoryStream stream)
    {
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
    }
}