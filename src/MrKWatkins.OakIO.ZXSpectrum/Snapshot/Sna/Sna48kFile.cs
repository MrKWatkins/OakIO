namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

// ReSharper disable once InconsistentNaming
/// <summary>
/// A 48K SNA snapshot file.
/// </summary>
public sealed class Sna48kFile : SnaFile
{
    private readonly byte[] ram;

    internal Sna48kFile(SnaHeader header, byte[] ram)
        : base(header)
    {
        this.ram = ram;
    }

    /// <summary>
    /// Creates a new 48K SNA file from the given memory contents.
    /// </summary>
    /// <param name="memory">The 64K memory contents to create the snapshot from.</param>
    /// <returns>The new 48K SNA file.</returns>
    [Pure]
    public static Sna48kFile Create(Span<byte> memory)
    {
        var header = new SnaHeader();
        var ram = memory[16384..].ToArray();
        return new Sna48kFile(header, ram);
    }

    /// <summary>
    /// Gets the 48K RAM data.
    /// </summary>
    public ReadOnlySpan<byte> Ram => ram;

    /// <inheritdoc />
    public override bool TryLoadInto(Span<byte> memory)
    {
        ram.CopyTo(memory[16384..]);
        return true;
    }
}