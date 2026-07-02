namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// A ZX Spectrum Next NEX snapshot file.
/// </summary>
public sealed class NexFile : ZXSpectrumSnapshotFile
{
    private readonly byte[]? palette;
    private readonly byte[]? copperCode;

    internal NexFile(NexHeader header, byte[]? palette, IReadOnlyList<NexScreen> screens, byte[]? copperCode, IReadOnlyList<NexBank> banks)
        : base(NexFormat.Instance)
    {
        Header = header;
        this.palette = palette;
        Screens = screens;
        this.copperCode = copperCode;
        Banks = banks;
    }

    /// <summary>
    /// Gets the NEX file header.
    /// </summary>
    public NexHeader Header { get; }

    /// <summary>
    /// Gets the palette data, or <c>null</c> if the file has no palette.
    /// </summary>
    public IReadOnlyList<byte>? Palette => palette;

    internal ReadOnlyMemory<byte> PaletteMemory => palette;

    /// <summary>
    /// Gets the loading screens.
    /// </summary>
    public IReadOnlyList<NexScreen> Screens { get; }

    /// <summary>
    /// Gets the copper code data, or <c>null</c> if the file has no copper code.
    /// </summary>
    public IReadOnlyList<byte>? CopperCode => copperCode;

    internal ReadOnlyMemory<byte> CopperCodeMemory => copperCode;

    /// <summary>
    /// Gets the memory banks.
    /// </summary>
    public IReadOnlyList<NexBank> Banks { get; }

    /// <inheritdoc />
    public override RegisterSnapshot Registers => Header.Registers;
}