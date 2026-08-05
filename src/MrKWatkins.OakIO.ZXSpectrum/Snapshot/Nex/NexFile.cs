namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// A ZX Spectrum Next NEX snapshot file.
/// </summary>
public sealed class NexFile : ZXSpectrumSnapshotFile
{
    private const int BankSize = 16384;

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
    /// Creates a NEX file containing code banks, with no loading screens, palette or copper code.
    /// </summary>
    /// <param name="banks">
    /// The banks to include, each with a bank number and up to 16384 bytes of data; shorter data is zero padded to a
    /// full bank.
    /// </param>
    /// <param name="pc">The program counter to start execution at.</param>
    /// <param name="sp">The initial stack pointer.</param>
    /// <param name="borderColour">The border colour whilst loading.</param>
    /// <param name="ramRequired">The RAM the file requires.</param>
    /// <returns>A new <see cref="NexFile" /> containing the banks.</returns>
    /// <exception cref="ArgumentException">
    /// A bank's data is bigger than a bank, or the same bank number is included more than once.
    /// </exception>
    [Pure]
    public static NexFile CreateCode(
        [InstantHandle] IEnumerable<(int BankNumber, byte[] Data)> banks,
        ushort pc,
        ushort sp,
        ZXColour borderColour = ZXColour.Black,
        NexRamRequired ramRequired = NexRamRequired.Ram768K)
    {
        var header = new NexHeader
        {
            RamRequired = ramRequired,
            BorderColour = borderColour,
            SP = sp,
            PC = pc
        };

        var nexBanks = new List<NexBank>();
        foreach (var (bankNumber, data) in banks)
        {
            if (data.Length > BankSize)
            {
                throw new ArgumentException($"Bank {bankNumber} has {data.Length} bytes; a bank holds {BankSize}.", nameof(banks));
            }

            if (header.IsBankIncluded(bankNumber))
            {
                throw new ArgumentException($"Bank {bankNumber} is included more than once.", nameof(banks));
            }

            var padded = new byte[BankSize];
            data.CopyTo(padded, 0);
            nexBanks.Add(new NexBank(bankNumber, padded));
            header.SetBankIncluded(bankNumber, true);
        }

        header.NumBanksToLoad = (byte)nexBanks.Count;

        return new NexFile(header, null, [], null, nexBanks);
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