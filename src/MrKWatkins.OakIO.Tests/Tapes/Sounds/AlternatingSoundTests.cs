using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tests.Tapes.Sounds;

public sealed class AlternatingSoundTests
{
    [Test]
    public void Constructor()
    {
        var sound = new Pulse(100);
        var alternating = new AlternatingSound(50, sound);

        alternating.Repeats.Should().Equal(50);
        alternating.Sound.Should().Equal(sound);
        alternating.ToString().Should().Equal("50 x P:100");
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_InvalidRepeats(int repeats)
    {
        var sound = new Pulse(100);
        AssertThat.Invoking(() => new AlternatingSound(repeats, sound)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Start([Values] bool signal)
    {
        var sound = new Pulse(100);
        var alternating = new AlternatingSound(50, sound);
        alternating.Start(signal);

        alternating.Signal.Should().Equal(signal);
        alternating.RepeatsRemaining.Should().Equal(49);
        sound.Signal.Should().Equal(signal);
        sound.TStatesRemaining.Should().Equal(100);
    }

    [TestCaseSource(nameof(AdvanceTestCases))]
    public void Advance(int repeats, int pulseLength, AdvanceTestStep[] steps)
    {
        var sound = new Pulse(pulseLength);
        var alternating = new AlternatingSound(repeats, sound);
        alternating.Start(true);

        foreach (var step in steps)
        {
            alternating.Advance(step.AdvanceBy).Should().Equal(step.ExpectedTStatesLeftOver);

            alternating.Signal.Should().Equal(step.ExpectedSignal);
            alternating.RepeatsRemaining.Should().Equal(step.ExpectedRepeatsRemaining);
            sound.TStatesRemaining.Should().Equal(step.ExpectedPulseTStatesRemaining);
        }
    }

    [Pure]
    public static IEnumerable<TestCaseData> AdvanceTestCases()
    {
        yield return new TestCaseData(1, 100, new AdvanceTestStep[]
        {
            new(0, 0, true, 0, 100),
            new(60, 0, true, 0, 40),
            new(60, 20, true, 0, 0)
        }).SetArgDisplayNames("1 repeat");

        yield return new TestCaseData(1, 100, new AdvanceTestStep[]
        {
            new(0, 0, true, 0, 100),
            new(40, 0, true, 0, 60),
            new(60, 0, true, 0, 0)
        }).SetArgDisplayNames("1 repeat, finish sound exactly");

        yield return new TestCaseData(2, 100, new AdvanceTestStep[]
        {
            new(0, 0, true, 1, 100),
            new(60, 0, true, 1, 40),
            new(60, 0, false, 0, 80),
            new(60, 0, false, 0, 20),
            new(60, 40, false, 0, 00)
        }).SetArgDisplayNames("2 repeats");

        yield return new TestCaseData(2, 100, new AdvanceTestStep[]
        {
            new(0, 0, true, 1, 100),
            new(40, 0, true, 1, 60),
            new(60, 0, true, 1, 0),
            new(1, 0, false, 0, 99),
            new(99, 0, false, 0, 0),
            new(10, 10, false, 0, 0)
        }).SetArgDisplayNames("2 repeats, finish sound exactly");

        yield return new TestCaseData(3, 100, new AdvanceTestStep[]
        {
            new(90, 0, true, 2, 10),
            new(90, 0, false, 1, 20),
            new(90, 0, true, 0, 30),
            new(90, 60, true, 0, 0)
        }).SetArgDisplayNames("3 repeats");

    }

    public sealed record AdvanceTestStep(int AdvanceBy, int ExpectedTStatesLeftOver, bool ExpectedSignal, int ExpectedRepeatsRemaining, int ExpectedPulseTStatesRemaining);
}