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
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var headerBytes = await reader.ReadAsync(27).ConfigureAwait(false);
        var rest = await reader.ReadToEndAsync().ConfigureAwait(false);

        return rest.Length == 49152
            ? Read48k(headerBytes, rest)
            : Read128k(headerBytes, rest);
    }

    [MustUseReturnValue]
    private static Sna48kFile Read48k(byte[] headerBytes, byte[] ram)
    {
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
    private static Sna128kFile Read128k(byte[] headerBytes, byte[] rest)
    {
        var banks = new byte[8][];
        banks[5] = rest[..16384];
        banks[2] = rest[16384..32768];
        var pagedBankData = rest[32768..49152];
        var footerData = rest[49152..49156];

        var pagedBank = footerData[2] & 0x07;
        banks[pagedBank] = pagedBankData;

        var header = new SnaHeader(headerBytes, footerData);

        var offset = 49156;
        foreach (var bankNumber in new byte[] { 0, 1, 3, 4, 6, 7 })
        {
            if (bankNumber == pagedBank)
            {
                continue;
            }

            banks[bankNumber] = rest[offset..(offset + 16384)];
            offset += 16384;
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
        await writer.WriteAsync(file.GetBankMemory(5)).ConfigureAwait(false);
        await writer.WriteAsync(file.GetBankMemory(2)).ConfigureAwait(false);
        await writer.WriteAsync(file.GetBankMemory(file.PagedBank)).ConfigureAwait(false);

        var footer = new byte[4];
        footer.SetUInt16(0, file.Registers.PC);
        footer[2] = file.Port7FFD;
        footer[3] = file.TrDosRomPaged ? (byte)1 : (byte)0;
        await writer.WriteAsync(footer).ConfigureAwait(false);

        foreach (var bankNumber in new byte[] { 0, 1, 3, 4, 6, 7 })
        {
            if (bankNumber == file.PagedBank)
            {
                continue;
            }

            await writer.WriteAsync(file.GetBankMemory(bankNumber)).ConfigureAwait(false);
        }
    }
}