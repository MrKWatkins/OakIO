using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using TapeDataBlock = MrKWatkins.OakIO.Tape.DataBlock;
using TapePauseBlock = MrKWatkins.OakIO.Tape.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class TapToTapeConverterTests
{
    [Test]
    public void Convert_ReturnsBlocksForBlocks()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var tape = new TapToTapeConverter().Convert(tap);

        tape.Blocks.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_ProducesCorrectBlockSequence()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var tape = new TapToTapeConverter().Convert(tap);

        // Header block: pilot + data (no pause).
        // Data block: pilot + data + pause.
        tape.Blocks.Should().HaveCount(5);
        tape.Blocks[0].Should().BeOfType<SoundBlock>();    // Header pilot tone.
        tape.Blocks[1].Should().BeOfType<TapeDataBlock>(); // Header data bytes.
        tape.Blocks[2].Should().BeOfType<SoundBlock>();    // Data pilot tone (must NOT be a PauseBlock).
        tape.Blocks[3].Should().BeOfType<TapeDataBlock>(); // Program data bytes.
        tape.Blocks[4].Should().BeOfType<TapePauseBlock>(); // Pause after data block only.
    }

    [Test]
    public void Convert_PilotToneStartsLow()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tape = new TapToTapeConverter().Convert(tap);

        tape.Start();

        tape.Advance(1).Should().BeFalse();
    }
}