namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

// ReSharper disable once InconsistentNaming
public sealed class Sna128kFile : SnaFile
{
    private readonly byte[][] banks;
    private readonly byte[] footerData;

    internal Sna128kFile(SnaHeader header, byte[][] banks, byte[] footerData)
        : base(header)
    {
        this.banks = banks;
        this.footerData = footerData;
    }

    public ReadOnlySpan<byte> GetBank(int bankNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bankNumber);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bankNumber, 7);

        return banks[bankNumber];
    }

    // ReSharper disable once InconsistentNaming
    public byte Port7FFD => footerData[2];

    public bool TrDosRomPaged => footerData[3] != 0;

    public override bool TryLoadInto(Span<byte> memory)
    {
        banks[5].CopyTo(memory[0x4000..]);
        banks[2].CopyTo(memory[0x8000..]);
        banks[PagedBank].CopyTo(memory[0xC000..]);
        return true;
    }

    internal byte PagedBank => (byte)(footerData[2] & 0x07);
}