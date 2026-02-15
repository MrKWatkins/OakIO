using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tests.Tapes;

public sealed class DataBlockTests
{
    [Test]
    public void Constructor_Standard()
    {
        byte[] data = [0xAA];
        var block = new DataBlock(data);

        block.Data.Should().SequenceEqual(data);
        block.DataLength.Should().Equal(1);
    }

    [Test]
    public void Constructor_Custom()
    {
        byte[] data = [0xFF];
        var block = new DataBlock(data, Sound.StandardZeroBit(), Sound.StandardOneBit(), 500, 8, true);

        block.Data.Should().SequenceEqual(data);
        block.DataLength.Should().Equal(1);
    }

    [Test]
    public void Start_And_Advance_SingleByte()
    {
        // 0x80 = 10000000: 1 one-bit (3420 T-states) + 7 zero-bits (1710 each = 11970) + tail pulse (945).
        // Total = 3420 + 11970 + 945 = 16335.
        byte[] data = [0x80];
        var block = new DataBlock(data);
        block.Start(true);

        var totalTStates = AdvanceToEnd(block);

        totalTStates.Should().Equal(16335);
    }

    [Test]
    public void Advance_MultipleBytesWithTailPulse()
    {
        // 0xFF = 8 one-bits (3420 each = 27360), 0x00 = 8 zero-bits (1710 each = 13680) + tail (945).
        // Total = 27360 + 13680 + 945 = 41985.
        byte[] data = [0xFF, 0x00];
        var block = new DataBlock(data);
        block.Start(true);

        var totalTStates = AdvanceToEnd(block);

        totalTStates.Should().Equal(41985);
    }

    [Test]
    public void Advance_NoTailPulse()
    {
        // 0xAA = 10101010: 4 one-bits + 4 zero-bits = 4*3420 + 4*1710 = 13680 + 6840 = 20520.
        // No tail pulse.
        byte[] data = [0xAA];
        var block = new DataBlock(data, Sound.StandardZeroBit(), Sound.StandardOneBit(), 0);
        block.Start(true);

        var totalTStates = AdvanceToEnd(block);

        totalTStates.Should().Equal(20520);
    }

    [Test]
    public void Advance_UsedBitsInLastByte()
    {
        // 0xF0 with 4 used bits = top 4 bits = 1111: 4 one-bits (3420 each = 13680) + tail (945).
        // Total = 13680 + 945 = 14625.
        byte[] data = [0xF0];
        var block = new DataBlock(data, Sound.StandardZeroBit(), Sound.StandardOneBit(), 945, 4);
        block.Start(true);

        var totalTStates = AdvanceToEnd(block);

        totalTStates.Should().Equal(14625);
    }

    private static int AdvanceToEnd(DataBlock block)
    {
        var totalTStates = 0;
        while (block.Advance(1) == 0)
        {
            totalTStates++;
            if (totalTStates > 200000)
            {
                break;
            }
        }

        return totalTStates;
    }
}
