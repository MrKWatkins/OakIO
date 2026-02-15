using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tests.Tapes.Sounds;

public sealed class SoundSequenceTests
{
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