using MrKWatkins.OakIO.Tape;
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
    public void Convert_PulseSequenceBlock_ProducesOneSoundBlockPerPulse()
    {
        // A PULS block with 3 pulse entries (pilot + sync1 + sync2) should produce 3 SoundBlocks.
        using var stream = new MemoryStream();
        WritePzxHeader(stream);
        WritePulsBlock(stream);

        stream.Position = 0;
        var pzx = PzxFormat.Instance.Read(stream);

        var tape = new PzxToTapeConverter().Convert(pzx);

        tape.Blocks.Should().HaveCount(3);
        foreach (var block in tape.Blocks)
        {
            block.Should().BeOfType<SoundBlock>();
        }
    }

    [Test]
    public void Convert_PulseSequenceBlock_FirstSoundBlockStartsLow()
    {
        // Per PZX spec: "The pulse level is low at start of the block by default."
        using var stream = new MemoryStream();
        WritePzxHeader(stream);
        WritePulsBlock(stream);

        stream.Position = 0;
        var pzx = PzxFormat.Instance.Read(stream);

        var tape = new PzxToTapeConverter().Convert(pzx);
        tape.Start();

        tape.Advance(1).Should().BeFalse();
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

    // Writes a PULS block encoding 3 pulses: pilot (8063 repeats of 2168 T-states), sync1 (667), sync2 (735).
    private static void WritePulsBlock(MemoryStream stream)
    {
        stream.Write("PULS"u8);
        stream.Write([0x08, 0x00, 0x00, 0x00]); // Body size = 8 bytes.
        // Repeated pilot pulse encoded as (0x8000 | 8063) in little-endian, then duration.
        stream.Write([0x7F, 0x9F]);              // 0x9F7F LE = 0x8000 | 8063.
        stream.Write([0x78, 0x08]);              // 2168 T-states LE.
        stream.Write([0x9B, 0x02]);              // sync1: 667 T-states LE (single pulse).
        stream.Write([0xEF, 0x02]);              // sync2: 735 T-states LE (single pulse).
    }
}