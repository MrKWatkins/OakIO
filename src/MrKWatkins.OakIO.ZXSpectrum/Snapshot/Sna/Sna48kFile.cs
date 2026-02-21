namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

// ReSharper disable once InconsistentNaming
public sealed class Sna48kFile : SnaFile
{
    private readonly byte[] ram;

    internal Sna48kFile(SnaHeader header, byte[] ram)
        : base(header)
    {
        this.ram = ram;
    }

    [Pure]
    public static Sna48kFile Create(Span<byte> memory)
    {
        var header = new SnaHeader();
        var ram = memory[16384..].ToArray();
        return new Sna48kFile(header, ram);
    }

    public ReadOnlySpan<byte> Ram => ram;

    public override bool TryLoadInto(Span<byte> memory)
    {
        ram.CopyTo(memory[16384..]);
        return true;
    }
}