using MrKWatkins.OakIO.Tape;

namespace MrKWatkins.OakIO.Tests.Tapes;

public sealed class PauseBlockTests
{
    [Test]
    public void Constructor()
    {
        var pause = new PauseBlock(500);
        pause.LengthInTStates.Should().Equal(500);
    }

    [Test]
    public void OneSecond()
    {
        var pause = PauseBlock.OneSecond(3500000m);
        pause.LengthInTStates.Should().Equal(3500000);
    }

    [Test]
    public void Start([Values] bool signal)
    {
        var pause = new PauseBlock(100);
        pause.Start(signal);

        pause.Signal.Should().Equal(signal);
    }

    [Test]
    public void Advance_Partial()
    {
        var pause = new PauseBlock(100);
        pause.Start(true);

        pause.Advance(50).Should().Equal(0);
    }

    [Test]
    public void Advance_ExactFinish()
    {
        var pause = new PauseBlock(100);
        pause.Start(true);

        pause.Advance(100).Should().Equal(0);
    }

    [Test]
    public void Advance_Overflow()
    {
        var pause = new PauseBlock(100);
        pause.Start(true);

        pause.Advance(120).Should().Equal(20);
    }
}
