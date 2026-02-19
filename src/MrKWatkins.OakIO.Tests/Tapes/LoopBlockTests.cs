using MrKWatkins.OakIO.Tape;

namespace MrKWatkins.OakIO.Tests.Tapes;

public sealed class LoopBlockTests
{
    [Test]
    public void Constructor()
    {
        var inner = new PauseBlock(100);
        var loop = new LoopBlock(3, [inner]);

        loop.Loops.Should().Equal(3);
        loop.Blocks.Should().HaveCount(1);
        loop.Blocks[0].Should().BeTheSameInstanceAs(inner);
    }

    [Test]
    public void Start([Values] bool signal)
    {
        var inner = new PauseBlock(50, initialSignal: true);
        var loop = new LoopBlock(2, [inner]);
        loop.Start(signal);

        loop.Signal.Should().Equal(signal);
    }

    [Test]
    public void Advance_SingleBlockSingleLoop()
    {
        // loops=1 means: play once (currentLoop=0) then repeat once (currentLoop=1).
        // Total T-states = 100 * 2 = 200.
        var inner = new PauseBlock(100);
        var loop = new LoopBlock(1, [inner]);
        loop.Start(true);

        // Advance through first iteration (100 T-states) + part of second.
        loop.Advance(50).Should().Equal(0);
        loop.Advance(60).Should().Equal(0); // First iteration finishes, 10 leftover consumed by second.

        // Finish second iteration (90 T-states remaining).
        loop.Advance(90).Should().Equal(0);   // Pulse at 0 remaining.
        loop.Advance(10).Should().Equal(10);  // Second iteration done, leftover returned.
    }

    [Test]
    public void Advance_SingleBlockMultipleLoops()
    {
        // loops=2 means 3 total iterations = 300 T-states.
        var inner = new PauseBlock(100);
        var loop = new LoopBlock(2, [inner]);
        loop.Start(true);

        // Advance through all 3 iterations with large steps.
        loop.Advance(110).Should().Equal(0);  // First iteration done (100), 10 into second.
        loop.Advance(110).Should().Equal(0);  // Second done (90 remaining + 20 into third).
        loop.Advance(80).Should().Equal(0);   // Third at 0 remaining.
        loop.Advance(10).Should().Equal(10);  // Done.
    }

    [Test]
    public void Advance_MultipleBlocksMultipleLoops()
    {
        // loops=1: 2 total iterations, each with 2 blocks of 50 = 200 total T-states.
        var block1 = new PauseBlock(50);
        var block2 = new PauseBlock(50);
        var loop = new LoopBlock(1, [block1, block2]);
        loop.Start(true);

        // First loop, first block.
        loop.Advance(30).Should().Equal(0);

        // First loop, past first block into second block.
        loop.Advance(30).Should().Equal(0);

        // First loop, past second block into second loop, first block.
        loop.Advance(60).Should().Equal(0);

        // Second loop, past first block into second block.
        loop.Advance(60).Should().Equal(0);

        // Second loop, second block reaches 0.
        loop.Advance(20).Should().Equal(0);

        // Finish.
        loop.Advance(10).Should().Equal(10);
    }
}