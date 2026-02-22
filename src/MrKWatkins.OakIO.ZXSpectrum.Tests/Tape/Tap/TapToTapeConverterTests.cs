using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

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
}