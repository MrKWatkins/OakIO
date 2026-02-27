namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX loop start block.
/// </summary>
public sealed class LoopStartHeader : TzxBlockHeader
{
    private const int Size = 2;

    internal LoopStartHeader()
        : base(TzxBlockType.LoopStart, Size)
    {
    }

    internal LoopStartHeader(Stream stream)
        : base(TzxBlockType.LoopStart, Size, stream)
    {
    }

    /// <summary>
    /// Gets the number of times the loop should be repeated.
    /// </summary>
    public ushort NumberOfRepetitions => GetWord(0);

    /// <inheritdoc />
    public override string ToString() => $"{Type}: {NumberOfRepetitions} repetitions";
}