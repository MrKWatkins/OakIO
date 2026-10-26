namespace MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex;

/// <summary>
/// A ZX Spectrum Next NEX snapshot file.
/// </summary>
public sealed class NexFile : ZXSpectrumSnapshotFile
{
    private const int BankSize = 16384;

    private readonly byte[]? palette;
    private readonly byte[]? copperCode;
    private byte[] appendedData;

    internal NexFile(NexHeader header, byte[]? palette, IReadOnlyList<NexScreen> screens, byte[]? copperCode, IReadOnlyList<NexBank> banks, byte[] appendedData)
        : base(NexFormat.Instance)
    {
        Header = header;
        this.palette = palette;
        Screens = screens;
        this.copperCode = copperCode;
        Banks = banks;
        this.appendedData = appendedData;
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

        return new NexFile(header, null, [], null, nexBanks, []);
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

    /// <summary>
    /// Gets the opaque binary data following the memory banks, or an empty list when no data is appended.
    /// </summary>
    /// <remarks>
    /// The NEX loader does not load these bytes into RAM. Their format is application-defined.
    /// Format reference: https://wiki.specnext.dev/NEX_file_format.
    /// </remarks>
    public IReadOnlyList<byte> AppendedData => appendedData;

    internal ReadOnlyMemory<byte> AppendedDataMemory => appendedData;

    /// <summary>
    /// Gets the zero-based byte offset of the appended data in the serialised NEX file.
    /// </summary>
    /// <remarks>
    /// Relative to the start of the NEX header, independently of the input or output stream's position.
    /// When no data is appended, this is the position at which appended data would begin.
    /// </remarks>
    public long AppendedDataOffset => NexHeader.Size + (long)PaletteMemory.Length +
        Screens.Sum(screen => (long)screen.Data.Count) + CopperCodeMemory.Length +
        Banks.Sum(bank => (long)bank.Data.Count);

    /// <summary>
    /// Replaces the application-defined data appended after the memory banks.
    /// </summary>
    /// <param name="data">The bytes to copy, or an empty span to remove existing appended data.</param>
    /// <remarks>
    /// Copies the supplied bytes. Does not change the bank map, RAM requirement or loader file-handle policy.
    /// Set <see cref="NexHeader.FileHandleAddress" /> to keep the file open when the application needs to read it.
    /// Existing checksum fields are preserved, not recalculated.
    /// </remarks>
    public void SetAppendedData(ReadOnlySpan<byte> data) => appendedData = data.ToArray();

    /// <inheritdoc />
    public override RegisterSnapshot Registers => Header.Registers;
}