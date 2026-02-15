namespace MrKWatkins.OakIO.Tape;

public abstract class TapeBlock(bool? initialSignal = null)
{
    internal bool? InitialSignal { get; } = initialSignal;

    internal abstract bool Signal { get; }

    internal void Start(TapeBlock? previousBlock)
    {
        bool signal;
        if (InitialSignal.HasValue)
        {
            signal = InitialSignal.Value;
        }
        else if (previousBlock != null)
        {
            signal = !previousBlock.Signal;
        }
        else
        {
            signal = true;
        }

        Start(signal);
    }

    internal abstract void Start(bool signal);

    /// <summary>
    /// Advances the block by <paramref name="tStates" />. Returns 0 if the block is still in progress, or
    /// the number of T states left over if the block is finished.
    /// </summary>
    [MustUseReturnValue]
    internal abstract int Advance(int tStates);
}
