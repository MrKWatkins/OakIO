using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tests.Tapes.Sounds;

public sealed class SoundSequenceTests
{
    [Test]
    public void Constructor_EmptySoundsThrows()
    {
        AssertThat.Invoking(() => new SoundSequence()).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_IEnumerable()
    {
        var pulses = new List<Sound> { new Pulse(10), new Pulse(20) };
        var sequence = new SoundSequence(pulses);

        sequence.Sounds.Should().HaveCount(2);
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        var sequence = new SoundSequence(new Pulse(10), new Pulse(20));

        sequence.ToString().Should().Equal("[P:10, P:20]");
    }

    [Test]
    public void Advance()
    {
        var pulse0 = new Pulse(50);
        var pulse1 = new Pulse(75);
        var sequence = new SoundSequence(pulse0, pulse1);

        sequence.Start(false);
        sequence.CurrentSound.Should().Equal(pulse0);
        sequence.Signal.Should().BeFalse();

        sequence.Advance(1).Should().Equal(0);
        sequence.CurrentSound.Should().Equal(pulse0);

        sequence.Advance(49).Should().Equal(0);
        sequence.CurrentSound.Should().Equal(pulse0);
        pulse0.TStatesRemaining.Should().Equal(0);

        sequence.Advance(1).Should().Equal(0);
        sequence.CurrentSound.Should().Equal(pulse1);
        sequence.Signal.Should().BeTrue();

        sequence.Advance(74).Should().Equal(0);
        sequence.CurrentSound.Should().Equal(pulse1);
        pulse1.TStatesRemaining.Should().Equal(0);

        sequence.Advance(1).Should().Equal(1);
    }
}