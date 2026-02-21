namespace MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;

public sealed class SnaSnapshot128kFile : SnaSnapshotFile
{
    private readonly byte[][] banks;
    private readonly byte[] footerData;

    internal SnaSnapshot128kFile(SnaSnapshotHeader header, byte[][] banks, byte[] footerData)
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