using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tests.Tapes.Sounds;

public sealed class PulseTests
{
    [Test]
    public void Constructor()
    {
        var pulse = new Pulse(100);
        pulse.LengthInTStates.Should().Equal(100);
        pulse.ToString().Should().Equal("P:100");
    }

    [Test]
    public void Start([Values] bool signal)
    {
        var pulse = new Pulse(100);
        pulse.Start(signal);

        pulse.Signal.Should().Equal(signal);
        pulse.TStatesRemaining.Should().Equal(100);
    }

    [TestCase(0, 0, 100)]
    [TestCase(99, 0, 1)]
    [TestCase(100, 0, 0)]
    [TestCase(101, 1, 0)]
    public void Advance(int advanceBy, int expectedTStatesLeftOver, int expectedTStatesRemaining)
    {
        var pulse = new Pulse(100);
        pulse.Start(true);

        pulse.Advance(advanceBy).Should().Equal(expectedTStatesLeftOver);
        pulse.TStatesRemaining.Should().Equal(expectedTStatesRemaining);
    }
}