using MrKWatkins.OakIO.Tape;

namespace MrKWatkins.OakIO.Tests.Tapes;

public sealed class TapeBlockTests
{
    [Test]
    public void Start_WithInitialSignalTrue()
    {
        var block = new PauseBlock(100, initialSignal: true);
        block.Start(null);

        block.Signal.Should().BeTrue();
    }

    [Test]
    public void Start_WithInitialSignalFalse()
    {
        var block = new PauseBlock(100, initialSignal: false);
        block.Start(null);

        block.Signal.Should().BeFalse();
    }

    [Test]
    public void Start_InheritsFromPreviousBlock_InvertsSignal()
    {
        var previous = new PauseBlock(100, initialSignal: true);
        previous.Start(null);

        var block = new PauseBlock(100);
        block.Start(previous);

        // Should invert: previous is true, so this should be false.
        block.Signal.Should().BeFalse();
    }

    [Test]
    public void Start_NoPreviousBlock_NoInitialSignal_DefaultsToTrue()
    {
        var block = new PauseBlock(100);
        block.Start(null);

        block.Signal.Should().BeTrue();
    }
}