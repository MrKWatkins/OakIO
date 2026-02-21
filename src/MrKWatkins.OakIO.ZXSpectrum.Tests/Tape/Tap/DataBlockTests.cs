using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class DataBlockTests
{
    [Test]
    public void Create()
    {
        var block = DataBlock.Create([0xF3, 0xAF]);
        block.Length.Should().Equal(2);
    }

    [Test]
    public void ToString_ReturnsDataWithLength()
    {
        var block = DataBlock.Create([0xF3, 0xAF]);
        block.ToString().Should().Equal("Data: 2 bytes");
    }
}