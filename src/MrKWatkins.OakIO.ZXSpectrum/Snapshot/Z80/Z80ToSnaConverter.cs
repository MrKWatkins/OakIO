namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Converts a Z80 snapshot file to an SNA snapshot file.
/// </summary>
public sealed class Z80ToSnaConverter : IOFileConverter<Z80File, Sna.SnaFile>
{
    internal Z80ToSnaConverter()
        : base(Z80Format.Instance, Sna.SnaFormat.Instance)
    {
    }

    /// <inheritdoc />
    [Pure]
    public override Sna.SnaFile Convert(Z80File source)
    {
        Sna.SnaFile sna;
        if (source is IZ80SnapshotV2OrV3File { HardwareMode: HardwareMode.Spectrum128 } banked)
        {
            if (banked.Pages.Any(page => page.Header.PageNumber is < 3 or > 10))
            {
                throw new NotSupportedException("SNA cannot preserve additional ROM or peripheral memory pages.");
            }
            sna = Sna.Sna128kFile.Create(Enumerable.Range(0, 8)
                .Select(bank => (ReadOnlyMemory<byte>)banked.GetBank(bank).ToArray()).ToArray(), banked.Port7FFD);
        }
        else
        {
            var memory = new byte[65536];
            if (!source.TryLoadInto(memory))
            {
                throw new InvalidOperationException("Failed to load Z80 snapshot into memory.");
            }
            sna = Sna.Sna48kFile.Create(memory);
        }
        CopyRegisters(source.Registers, sna.Registers);
        sna.Header.BorderColour = source.Header.BorderColour;
        sna.Header.InterruptMode = source.Header.InterruptMode;
        sna.Header.IFF2 = source.Header.IFF2;

        return sna;
    }

    private static void CopyRegisters(RegisterSnapshot source, RegisterSnapshot target)
    {
        target.AF = source.AF;
        target.BC = source.BC;
        target.DE = source.DE;
        target.HL = source.HL;
        target.IX = source.IX;
        target.IY = source.IY;
        target.PC = source.PC;
        target.SP = source.SP;
        target.IR = source.IR;
        target.Shadow.AF = source.Shadow.AF;
        target.Shadow.BC = source.Shadow.BC;
        target.Shadow.DE = source.Shadow.DE;
        target.Shadow.HL = source.Shadow.HL;
    }
}