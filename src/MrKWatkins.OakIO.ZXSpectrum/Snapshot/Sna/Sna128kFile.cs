namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

// Bank ordering and footer: https://worldofspectrum.net/zx-modules/fileformats/snaformat.html
// ReSharper disable once InconsistentNaming
/// <summary>
/// A 128K SNA snapshot file.
/// </summary>
public sealed class Sna128kFile : SnaFile
{
    private readonly byte[][] banks;
    private readonly byte[] footerData;

    /// <summary>
    /// Creates a 128K snapshot from eight physical 16 KB RAM banks, numbered 0–7. Input data is copied.
    /// Registers are initially zero. The default paging latch selects the 48K ROM and RAM bank 0, with paging unlocked.
    /// This does not initialize BASIC system variables, including BANKM.
    /// </summary>
    [Pure]
    public static Sna128kFile Create(IReadOnlyList<ReadOnlyMemory<byte>> banks, byte port7FFD = 16, bool trDosRomPaged = false)
    {
        SnapshotBanks.Validate(banks);
        var footer = new byte[]
        {
            0, 0, port7FFD, trDosRomPaged ? (byte)1 : (byte)0
        };
        return new Sna128kFile(new SnaHeader(new byte[27], footer), banks.Select(bank => bank.ToArray()).ToArray(), footer);
    }

    internal Sna128kFile(SnaHeader header, byte[][] banks, byte[] footerData)
        : base(header)
    {
        this.banks = banks;
        this.footerData = footerData;
    }

    /// <summary>
    /// Gets the data for the specified memory bank.
    /// </summary>
    /// <param name="bankNumber">The bank number, from 0 to 7.</param>
    /// <returns>The bank data.</returns>
    [Pure]
    public ReadOnlySpan<byte> GetBank(int bankNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bankNumber);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bankNumber, 7);

        return banks[bankNumber];
    }

    [Pure]
    internal ReadOnlyMemory<byte> GetBankMemory(int bankNumber) => banks[bankNumber];

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Gets the value of port 0x7FFD.
    /// </summary>
    public byte Port7FFD => footerData[2];

    /// <summary>
    /// Gets a value indicating whether the TR-DOS ROM is paged in.
    /// </summary>
    public bool TrDosRomPaged => footerData[3] != 0;

    /// <inheritdoc />
    [MustUseReturnValue]
    public override bool TryLoadInto(Span<byte> memory)
    {
        banks[5].CopyTo(memory[0x4000..]);
        banks[2].CopyTo(memory[0x8000..]);
        banks[PagedBank].CopyTo(memory[0xC000..]);
        return true;
    }

    internal byte PagedBank => (byte)(footerData[2] & 0x07);
}