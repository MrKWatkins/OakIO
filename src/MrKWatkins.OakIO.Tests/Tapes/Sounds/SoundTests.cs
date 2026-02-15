using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tests.Tapes.Sounds;

public sealed class SoundTests
{
    [Test]
    public void StandardHeaderPureToneAndSync() => TestStandardPureToneAndSync(Sound.StandardHeaderPureToneAndSync(), 8065);

    [Test]
    public void StandardDataPureToneAndSync() => TestStandardPureToneAndSync(Sound.StandardDataPureToneAndSync(), 3225);

    private static void TestStandardPureToneAndSync(Sound sound, int expectedPolarities)
    {
        // From https://github.com/raxoft/pzxtools/blob/master/docs/pzx_format.txt:
        // "The pulse level is low at start of the block by default."
        // "For the record, the standard ROM save routines create the pilot tone in such
        // a way that the level of the first sync pulse is high and the level of the
        // second sync pulse is low."
        var polarities = GetPolarities(sound, false);
        polarities.Should().HaveCount(expectedPolarities);

        var firstSyncPolarity = polarities[^2];
        firstSyncPolarity.Should().BeTrue();

        var secondSyncPolarity = polarities[^1];
        secondSyncPolarity.Should().BeFalse();
    }

    [Test]
    public void PureTone()
    {
        var sound = Sound.PureTone(3, 100);
        sound.Start(true);

        // 3 repeats of a pulse with 100 T-states each = 300 total T-states.
        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        totalTStates.Should().Equal(300);
    }

    [Test]
    public void Bit()
    {
        var sound = Sound.Bit(200);
        sound.Start(false);

        // A bit is two pulses of the same length.
        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        totalTStates.Should().Equal(400);
    }

    [Test]
    public void StandardZeroBit()
    {
        var sound = Sound.StandardZeroBit();
        sound.Start(true);

        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        // Standard zero bit = 2 x 855 = 1710.
        totalTStates.Should().Equal(1710);
    }

    [Test]
    public void StandardOneBit()
    {
        var sound = Sound.StandardOneBit();
        sound.Start(true);

        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        // Standard one bit = 2 x 1710 = 3420.
        totalTStates.Should().Equal(3420);
    }

    [Test]
    public void PulseSequence_IEnumerable()
    {
        var lengths = new List<ushort> { 100, 200 };
        var sound = Sound.PulseSequence(lengths);
        sound.Start(true);

        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        totalTStates.Should().Equal(300);
    }

    [Test]
    public void PulseSequence_ReadOnlySpan()
    {
        ReadOnlySpan<ushort> lengths = [100, 200];
        var sound = Sound.PulseSequence(lengths);
        sound.Start(true);

        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        totalTStates.Should().Equal(300);
    }

    [Test]
    public void PureToneAndSync_NoSyncPulses()
    {
        var sound = Sound.PureToneAndSync(2, 100, 0, 0);
        sound.Start(true);

        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        // Just the pure tone: 2 x 100 = 200.
        totalTStates.Should().Equal(200);
    }

    [Test]
    public void PureToneAndSync_OnlyFirstSyncPulse()
    {
        var sound = Sound.PureToneAndSync(2, 100, 50, 0);
        sound.Start(true);

        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        // Pure tone: 2 x 100 = 200, first sync: 50, total = 250.
        totalTStates.Should().Equal(250);
    }

    [Test]
    public void PureToneAndSync_OnlySecondSyncPulse()
    {
        var sound = Sound.PureToneAndSync(2, 100, 0, 50);
        sound.Start(true);

        var totalTStates = 0;
        while (sound.Advance(1) == 0)
        {
            totalTStates++;
        }

        // Pure tone: 2 x 100 = 200, second sync: 50, total = 250.
        totalTStates.Should().Equal(250);
    }

    [Pure]
    private static IReadOnlyList<bool> GetPolarities(Sound sound, bool startPolarity)
    {
        sound.Start(startPolarity);

        var currentPolarity = sound.Signal;
        var polarities = new List<bool> { currentPolarity };

        while (sound.Advance(1) == 0)
        {
            if (sound.Signal != currentPolarity)
            {
                polarities.Add(sound.Signal);
                currentPolarity = sound.Signal;
            }
        }

        return polarities;
    }
}