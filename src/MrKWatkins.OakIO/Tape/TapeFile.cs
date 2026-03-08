namespace MrKWatkins.OakIO.Tape;

/// <summary>
/// A generic tape file composed of a sequence of blocks.
/// </summary>
public sealed class TapeFile : IOFile
{
    private readonly IReadOnlyList<TapeBlock> allBlocks;

    internal TapeFile(IReadOnlyList<TapeBlock> blocks)
        : base(TapeFormat.Instance)
    {
        var blocksList = blocks.ToList();
        Blocks = blocksList.AsReadOnly();

        var withFinished = new List<TapeBlock>(blocksList) { new FinishedBlock() };
        allBlocks = withFinished;
    }

    /// <summary>
    /// Gets the blocks in the tape file.
    /// </summary>
    public IReadOnlyList<TapeBlock> Blocks { get; }

    internal int Position { get; private set; }

    internal TapeBlock CurrentBlock => allBlocks[Position];

    /// <summary>
    /// Starts playback from the beginning of the tape.
    /// </summary>
    public void Start()
    {
        Position = 0;
        CurrentBlock.Start(null);
    }

    /// <summary>
    /// Advances the tape by <paramref name="tStates" /> T-states, returning the current signal level.
    /// </summary>
    /// <param name="tStates">The number of T-states to advance.</param>
    /// <returns>The current signal level after advancing.</returns>
    // Assumes we'll never have a tStates greater than a pulse.
    public bool Advance(int tStates)
    {
        var block = CurrentBlock;
        var tStatesLeftOver = block.Advance(tStates);
        if (tStatesLeftOver == 0)
        {
            return block.Signal;
        }

        // Move to the next block.
        Position++;

        CurrentBlock.Start(block);

        return Advance(tStatesLeftOver);
    }

    /// <summary>
    /// Gets a value indicating whether the tape has reached the end.
    /// </summary>
    public bool IsFinished => CurrentBlock is FinishedBlock;
}