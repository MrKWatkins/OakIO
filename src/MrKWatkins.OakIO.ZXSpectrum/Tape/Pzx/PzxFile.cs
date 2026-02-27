namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// A PZX tape image file.
/// </summary>
public sealed class PzxFile : ZXSpectrumTapeFile
{
    internal PzxFile(IReadOnlyList<PzxBlock> blocks)
        : base(PzxFormat.Instance)
    {
        Blocks = blocks;
    }

    /// <summary>
    /// Gets the blocks in this PZX file.
    /// </summary>
    public IReadOnlyList<PzxBlock> Blocks { get; }

    /// <inheritdoc />
    public override bool TryLoadInto(Span<byte> memory)
    {
        throw new NotImplementedException();
    }
}