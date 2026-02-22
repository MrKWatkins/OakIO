using MrKWatkins.OakIO.Tape;

namespace MrKWatkins.OakIO.Tests.Tapes;


public sealed class TapeFileTests
{
    [Test]
    public void Constructor_NoBlocks()
    {
        var tape = new TapeFile([]);

        tape.Blocks.Should().HaveCount(0);
        tape.Format.Should().BeTheSameInstanceAs(TapeFormat.Instance);
    }

    [Test]
    public void Constructor_WithBlocks()
    {
        var pause = new PauseBlock(100);
        var tape = new TapeFile([pause]);

        tape.Blocks.Should().HaveCount(1);
        tape.Blocks[0].Should().BeTheSameInstanceAs(pause);
    }

    [Test]
    public void Start_SetsPositionToZero()
    {
        var tape = new TapeFile([new PauseBlock(100)]);

        tape.Start();

        tape.Position.Should().Equal(0);
        tape.IsFinished.Should().BeFalse();
    }

    [Test]
    public void IsFinished_TrueWhenAllBlocksDone()
    {
        var tape = new TapeFile([]);

        tape.Start();

        tape.IsFinished.Should().BeTrue();
    }

    [Test]
    public void Advance_ThroughSingleBlock()
    {
        var tape = new TapeFile([new PauseBlock(100)]);

        tape.Start();
        tape.IsFinished.Should().BeFalse();

        // Advance partially.
        tape.Advance(50);
        tape.IsFinished.Should().BeFalse();

        // Advance to bring remaining to 0. The pulse has 0 T-states remaining but isn't "finished" yet.
        tape.Advance(50);
        tape.IsFinished.Should().BeFalse();

        // One more advance detects completion and moves to finished block.
        tape.Advance(1);
        tape.IsFinished.Should().BeTrue();
    }

    [Test]
    public void Advance_ThroughMultipleBlocks()
    {
        var tape = new TapeFile([new PauseBlock(50), new PauseBlock(50)]);

        tape.Start();
        tape.IsFinished.Should().BeFalse();

        // Advance past first block into second.
        tape.Advance(60);
        tape.Position.Should().Equal(1);
        tape.IsFinished.Should().BeFalse();

        // Advance second block to 0 remaining.
        tape.Advance(40);
        tape.IsFinished.Should().BeFalse();

        // One more advance to detect completion.
        tape.Advance(1);
        tape.IsFinished.Should().BeTrue();
    }

}