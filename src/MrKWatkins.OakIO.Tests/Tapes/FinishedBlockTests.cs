using MrKWatkins.OakIO.Tape;

namespace MrKWatkins.OakIO.Tests.Tapes;

public sealed class FinishedBlockTests
{
    [Test]
    public void Start([Values] bool signal)
    {
        var block = new FinishedBlock();
        block.Start(signal);

        block.Signal.Should().Equal(signal);
    }

    [Test]
    public void Advance_AlwaysReturnsZero()
    {
        var block = new FinishedBlock();
        block.Start(true);

        block.Advance(100).Should().Equal(0);
        block.Advance(0).Should().Equal(0);
    }
}
