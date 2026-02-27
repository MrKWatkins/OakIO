namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

/// <summary>
/// Converts SNA snapshot files to Z80 format.
/// </summary>
public sealed class SnaToZ80Converter : IOFileConverter<SnaFile, Z80.Z80File>
{
    internal SnaToZ80Converter()
        : base(SnaFormat.Instance, Z80.Z80Format.Instance)
    {
    }

    /// <inheritdoc />
    [Pure]
    public override Z80.Z80File Convert(SnaFile source)
    {
        var memory = new byte[65536];
        if (!source.TryLoadInto(memory))
        {
            throw new InvalidOperationException("Failed to load SNA snapshot into memory.");
        }

        var z80 = Z80.Z80V2File.Create48k(memory);
        CopyRegisters(source.Registers, z80.Registers);
        z80.Header.BorderColour = source.Header.BorderColour;
        z80.Header.InterruptMode = source.Header.InterruptMode;
        z80.Header.InterruptFlipFlop = source.Header.IFF2;
        z80.Header.IFF2 = source.Header.IFF2;

        return z80;
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