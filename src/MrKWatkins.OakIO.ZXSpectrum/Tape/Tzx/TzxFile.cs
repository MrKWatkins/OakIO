namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Represents a TZX tape file containing blocks of tape data.
/// </summary>
public sealed class TzxFile : ZXSpectrumTapeFile
{
    internal TzxFile(TzxHeader header, IReadOnlyList<TzxBlock> blocks)
        : base(TzxFormat.Instance)
    {
        Header = header;
        Blocks = blocks;
    }

    /// <summary>
    /// Gets the file header.
    /// </summary>
    public TzxHeader Header { get; }

    /// <summary>
    /// Gets the blocks in this TZX file.
    /// </summary>
    public IReadOnlyList<TzxBlock> Blocks { get; }

    /// <inheritdoc />
    public override bool TryLoadInto(Span<byte> memory)
    {
        throw new NotImplementedException();
    }
}