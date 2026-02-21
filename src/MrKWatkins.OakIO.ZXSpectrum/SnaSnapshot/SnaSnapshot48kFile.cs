namespace MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;

// ReSharper disable once InconsistentNaming
public sealed class SnaSnapshot48kFile : SnaSnapshotFile
{
    private readonly byte[] ram;

    internal SnaSnapshot48kFile(SnaSnapshotHeader header, byte[] ram)
        : base(header)
    {
        this.ram = ram;
    }

    [Pure]
    public static SnaSnapshot48kFile Create(Span<byte> memory)
    {
        var header = new SnaSnapshotHeader();
        var ram = memory[16384..].ToArray();
        return new SnaSnapshot48kFile(header, ram);
    }

    public ReadOnlySpan<byte> Ram => ram;

    public override bool TryLoadInto(Span<byte> memory)
    {
        ram.CopyTo(memory[16384..]);
        return true;
    }
}