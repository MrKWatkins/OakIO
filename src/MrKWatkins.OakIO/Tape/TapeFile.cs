namespace MrKWatkins.OakIO.Tape;

public sealed partial class TapeFile : IOFile
{
    private readonly IReadOnlyList<TapeBlock> allBlocks;

    public TapeFile([InstantHandle] IEnumerable<TapeBlock> blocks)
        : base(TapeFormat.Instance)
    {
        var blocksList = blocks.ToList();
        Blocks = blocksList.AsReadOnly();

        var withFinished = new List<TapeBlock>(blocksList) { new FinishedBlock() };
        allBlocks = withFinished;
    }

    public IReadOnlyList<TapeBlock> Blocks { get; }

    internal int Position { get; private set; }

    internal TapeBlock CurrentBlock => allBlocks[Position];

    internal void Start()
    {
        Position = 0;
        CurrentBlock.Start(null);
    }

    // Assumes we'll never have a tStates greater than a pulse.
    internal bool Advance(int tStates)
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

    internal bool IsFinished => CurrentBlock is FinishedBlock;
}
