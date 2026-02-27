namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// A ZX Spectrum Next NEX snapshot file.
/// </summary>
public sealed class NexFile : ZXSpectrumSnapshotFile
{
    private readonly NexRegisterSnapshot registers;

    internal NexFile(NexHeader header, byte[]? palette, IReadOnlyList<NexScreen> screens, byte[]? copperCode, IReadOnlyList<NexBank> banks)
        : base(NexFormat.Instance)
    {
        Header = header;
        Palette = palette;
        Screens = screens;
        CopperCode = copperCode;
        Banks = banks;
        registers = new NexRegisterSnapshot(header);
    }

    /// <summary>
    /// Gets the NEX file header.
    /// </summary>
    public NexHeader Header { get; }

    /// <summary>
    /// Gets the palette data, or <c>null</c> if the file has no palette.
    /// </summary>
    public byte[]? Palette { get; }

    /// <summary>
    /// Gets the loading screens.
    /// </summary>
    public IReadOnlyList<NexScreen> Screens { get; }

    /// <summary>
    /// Gets the copper code data, or <c>null</c> if the file has no copper code.
    /// </summary>
    public byte[]? CopperCode { get; }

    /// <summary>
    /// Gets the memory banks.
    /// </summary>
    public IReadOnlyList<NexBank> Banks { get; }

    /// <inheritdoc />
    public override RegisterSnapshot Registers => registers;
}