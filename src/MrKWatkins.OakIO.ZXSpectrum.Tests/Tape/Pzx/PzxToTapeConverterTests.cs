using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Pzx;

public sealed class PzxToTapeConverterTests
{
    [Test]
    public void Convert_ReturnsBlocksForBlocks()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = new TapToPzxConverter().Convert(tap);

        var tape = new PzxToTapeConverter().Convert(pzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_PauseBlockWithZeroDurationProducesNoBlocks()
    {
        using var stream = new MemoryStream();
        WritePzxHeader(stream);

        stream.Write("PAUS"u8);
        stream.Write([0x04, 0x00, 0x00, 0x00]);
        stream.Write([0x00, 0x00, 0x00, 0x00]);

        stream.Position = 0;
        var pzx = PzxFormat.Instance.Read(stream);

        var tape = new PzxToTapeConverter().Convert(pzx);

        tape.Blocks.Should().BeEmpty();
    }

    [Test]
    public void Convert_DataBlock_WithExtraBits()
    {
        // Covers the "ExtraBits > 0" branch (the pulse sequences have non-zero length as in the standard case).
        using var stream = new MemoryStream();
        WritePzxHeader(stream);

        // DATA block with 1 zero-bit pulse, 1 one-bit pulse, 5 ExtraBits, 1 byte data.
        // Size = 8 (fixed header portion after size field) + 2 + 2 + 1 = 13.
        stream.Write("DATA"u8);
        stream.Write([0x0D, 0x00, 0x00, 0x00]); // size = 13
        stream.Write([0x05, 0x00, 0x00, 0x00]); // SizeInBits = 5, ExtraBits = 5
        stream.Write([0x00, 0x00]);              // Tail = 0
        stream.WriteByte(0x01);                  // 1 zero-bit pulse
        stream.WriteByte(0x01);                  // 1 one-bit pulse
        stream.Write([0x57, 0x03]);              // zero-bit: 855 T-states
        stream.Write([0xAE, 0x06]);              // one-bit: 1710 T-states
        stream.WriteByte(0xFF);                  // 1 byte of data

        stream.Position = 0;
        var pzx = PzxFormat.Instance.Read(stream);

        var tape = new PzxToTapeConverter().Convert(pzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_DataBlock_WithStandardPulseSequences()
    {
        // Covers the zeroBitSound/oneBitSound "Length == 0" branches (use StandardZeroBit/StandardOneBit).
        using var stream = new MemoryStream();
        WritePzxHeader(stream);

        // DATA block with 0 zero-bit pulses, 0 one-bit pulses, 1 byte data.
        // Size = 8 (fixed header portion) + 0 + 0 + 1 = 9.
        stream.Write("DATA"u8);
        stream.Write([0x09, 0x00, 0x00, 0x00]); // size = 9
        stream.Write([0x08, 0x00, 0x00, 0x00]); // SizeInBits = 8, ExtraBits = 0
        stream.Write([0x00, 0x00]);              // Tail = 0
        stream.WriteByte(0x00);                  // 0 zero-bit pulses (use standard)
        stream.WriteByte(0x00);                  // 0 one-bit pulses (use standard)
        stream.WriteByte(0xFF);                  // 1 byte of data

        stream.Position = 0;
        var pzx = PzxFormat.Instance.Read(stream);

        var tape = new PzxToTapeConverter().Convert(pzx);

        tape.Blocks.Should().NotBeEmpty();
    }

    private static void WritePzxHeader(MemoryStream stream)
    {
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
    }
}