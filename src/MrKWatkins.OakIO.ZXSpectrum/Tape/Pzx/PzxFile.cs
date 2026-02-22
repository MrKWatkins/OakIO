namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

public sealed class PzxFile : ZXSpectrumTapeFile
{
    internal PzxFile(IReadOnlyList<PzxBlock> blocks)
        : base(PzxFormat.Instance)
    {
        Blocks = blocks;
    }

    public IReadOnlyList<PzxBlock> Blocks { get; }

    public override bool TryLoadInto(Span<byte> memory)
    {
        throw new NotImplementedException();
    }
}