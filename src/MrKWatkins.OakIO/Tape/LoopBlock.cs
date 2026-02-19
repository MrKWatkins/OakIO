namespace MrKWatkins.OakIO.Tape;

public sealed class LoopBlock(int loops, IReadOnlyList<TapeBlock> blocks) : TapeBlock(blocks[0].InitialSignal)
{
    private int currentLoop;
    private int currentBlock;

    public int Loops { get; } = loops;

    public IReadOnlyList<TapeBlock> Blocks { get; } = blocks;

    internal override bool Signal => Blocks[currentBlock].Signal;

    internal override void Start(bool signal)
    {
        currentLoop = 0;
        currentBlock = 0;
        Blocks[0].Start(signal);
    }

    internal override int Advance(int tStates)
    {
        var block = Blocks[currentBlock];
        var tStatesLeftOver = block.Advance(tStates);
        if (tStatesLeftOver == 0)
        {
            return 0;
        }

        // Have we finished the current loop?
        if (currentBlock == Blocks.Count - 1)
        {
            // Yes. Have we finished all the loops?
            if (currentLoop == Loops)
            {
                // Yes, we're done.
                return tStatesLeftOver;
            }

            // Move to the next loop.
            currentLoop++;
            currentBlock = 0;
        }
        else
        {
            // Move to the next block.
            currentBlock++;
        }

        Blocks[currentBlock].Start(block);
        return Advance(tStatesLeftOver);
    }
}