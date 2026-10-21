using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class TapBasBlockTests
{
    [Test]
    public void Constructor()
    {
        var block = new TapBasBlock(0x8000, new byte[]
        {
            12, 34
        });
        block.Location.Should().Equal((ushort)0x8000);
        block.Data.ToArray().Should().SequenceEqual(12, 34);
        block.Bank.Should().BeNull();
        var banked = block with
        {
            Bank = 7
        };
        banked.Bank.Should().Equal(7);
        block.Bank.Should().BeNull();
    }
}