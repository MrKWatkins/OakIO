using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

// https://worldofspectrum.net/zx-modules/fileformats/snaformat.html
/// <summary>
/// The SNA snapshot file format.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class SnaFormat : ZXSpectrumSnapshotFormat<SnaFile>
{
    /// <summary>
    /// The singleton instance of the SNA format.
    /// </summary>
    public static readonly SnaFormat Instance = new();

    private SnaFormat()
        : base("SNA Snapshot", "sna")
    {
    }

    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        yield return new SnaToZ80Converter();
    }

    /// <inheritdoc />
    protected override SnaFile ReadSnapshot(Stream stream)
    {
        var headerBytes = new byte[27];
        stream.ReadExactly(headerBytes, 0, 27);

        var remaining = stream.Length - stream.Position;
        return remaining == 49152
            ? Read48k(stream, headerBytes)
            : Read128k(stream, headerBytes);
    }

    [MustUseReturnValue]
    private static Sna48kFile Read48k(Stream stream, byte[] headerBytes)
    {
        var ram = new byte[49152];
        stream.ReadExactly(ram);

        // In 48K SNA files, the PC is stored on the stack. Pop it from SP and increment SP.
        var sp = headerBytes.GetUInt16(23);
        var pc = ram.GetUInt16(sp - 16384);
        sp += 2;
        headerBytes.SetUInt16(23, sp);

        var footerData = new byte[2];
        footerData.SetUInt16(0, pc);

        var header = new SnaHeader(headerBytes, footerData);
        return new Sna48kFile(header, ram);
    }

    [MustUseReturnValue]
    private static Sna128kFile Read128k(Stream stream, byte[] headerBytes)
    {
        var banks = Enumerable.Range(0, 8).Select(_ => new byte[16384]).ToArray();

        stream.ReadExactly(banks[5]);
        stream.ReadExactly(banks[2]);

        var pagedBankData = new byte[16384];
        stream.ReadExactly(pagedBankData);

        var footerData = new byte[4];
        stream.ReadExactly(footerData, 0, 4);

        var pagedBank = footerData[2] & 0x07;
        banks[pagedBank] = pagedBankData;

        var header = new SnaHeader(headerBytes, footerData);

        foreach (var bankNumber in new byte[] { 0, 1, 3, 4, 6, 7 })
        {
            if (bankNumber == pagedBank)
            {
                continue;
            }

            stream.ReadExactly(banks[bankNumber]);
        }

        return new Sna128kFile(header, banks, footerData);
    }

    /// <inheritdoc />
    protected override ValueTask WriteAsync(SnaFile file, IBinaryWriter writer) =>
        file switch
        {
            Sna48kFile file48K => Write48kAsync(file48K, writer),
            Sna128kFile file128K => Write128kAsync(file128K, writer),
            _ => throw new NotSupportedException($"The SNA file type {file.GetType().Name} is not supported.")
        };

    private static async ValueTask Write48kAsync(Sna48kFile file, IBinaryWriter writer)
    {
        // In 48K SNA files, PC is pushed onto the stack.
        var sp = file.Header.Registers.SP;
        sp -= 2;

        // Write the header with adjusted SP.
        var headerBytes = file.Header.AsReadOnlySpan().ToArray();
        headerBytes.SetUInt16(23, sp);
        await writer.WriteAsync(headerBytes).ConfigureAwait(false);

        // Write the RAM with PC at SP-2.
        var ram = file.Ram.ToArray();
        ram.SetUInt16(sp - 16384, file.Header.Registers.PC);
        await writer.WriteAsync(ram).ConfigureAwait(false);
    }

    private static async ValueTask Write128kAsync(Sna128kFile file, IBinaryWriter writer)
    {
        await file.Header.WriteAsync(writer).ConfigureAwait(false);
        writer.WriteBytes(file.GetBank(5));
        writer.WriteBytes(file.GetBank(2));
        writer.WriteBytes(file.GetBank(file.PagedBank));

        writer.WriteUInt16LittleEndian(file.Registers.PC);
        writer.WriteByte(file.Port7FFD);
        writer.WriteByte(file.TrDosRomPaged ? (byte)1 : (byte)0);

        foreach (var bankNumber in new byte[] { 0, 1, 3, 4, 6, 7 })
        {
            if (bankNumber == file.PagedBank)
            {
                continue;
            }

            writer.WriteBytes(file.GetBank(bankNumber));
        }
    }
}