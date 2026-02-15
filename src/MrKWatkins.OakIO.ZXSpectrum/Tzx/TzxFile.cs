namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class TzxFile : TapeFile
{
    internal TzxFile(TzxHeader header, IReadOnlyList<TzxBlock> blocks)
        : base(TzxFormat.Instance)
    {
        Header = header;
        Blocks = blocks;
    }

    public TzxHeader Header { get; }

    public IReadOnlyList<TzxBlock> Blocks { get; }

    public override bool TryLoadInto(Span<byte> memory)
    {
        throw new NotImplementedException();
    }
}