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