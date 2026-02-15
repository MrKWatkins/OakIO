using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Tape.Sounds;

namespace MrKWatkins.OakIO.Tests.Tapes;

public sealed class SoundBlockTests
{
    [Test]
    public void Constructor()
    {
        var sound = new Pulse(100);
        var block = new SoundBlock(sound);

        block.Sound.Should().BeTheSameInstanceAs(sound);
    }

    [Test]
    public void Start([Values] bool signal)
    {
        var sound = new Pulse(100);
        var block = new SoundBlock(sound);
        block.Start(signal);

        block.Signal.Should().Equal(signal);
    }

    [Test]
    public void Advance()
    {
        var sound = new Pulse(100);
        var block = new SoundBlock(sound);
        block.Start(true);

        block.Advance(50).Should().Equal(0);
        block.Signal.Should().BeTrue();

        block.Advance(60).Should().Equal(10);
    }

    [Test]
    public void InitialSignal()
    {
        var sound = new Pulse(100);
        var block = new SoundBlock(sound, initialSignal: false);
        block.Start(null);

        block.Signal.Should().BeFalse();
    }
}
