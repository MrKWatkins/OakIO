using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;

// https://worldofspectrum.net/zx-modules/fileformats/snaformat.html
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class SnaSnapshotFormat : SnapshotFormat<SnaSnapshotFile>
{
    public static readonly SnaSnapshotFormat Instance = new();

    private SnaSnapshotFormat()
        : base("SNA Snapshot", "sna")
    {
    }

    protected override SnaSnapshotFile ReadSnapshot(Stream stream)
    {
        var headerBytes = new byte[27];
        stream.ReadExactly(headerBytes, 0, 27);

        var remaining = stream.Length - stream.Position;
        return remaining == 49152
            ? Read48k(stream, headerBytes)
            : Read128k(stream, headerBytes, remaining);
    }

    [MustUseReturnValue]
    private static SnaSnapshot48kFile Read48k(Stream stream, byte[] headerBytes)
    {
        var ram = new byte[49152];
        stream.ReadExactly(ram);

        // In 48K SNA files, the PC is stored on the stack. Pop it from SP and increment SP.
        var sp = headerBytes.GetWord(23);
        var pc = ram.GetWord(sp - 16384);
        sp += 2;
        headerBytes.SetWord(23, sp);

        var footerData = new byte[2];
        footerData.SetWord(0, pc);

        var header = new SnaSnapshotHeader(headerBytes, footerData);
        return new SnaSnapshot48kFile(header, ram);
    }

    [MustUseReturnValue]
    private static SnaSnapshot128kFile Read128k(Stream stream, byte[] headerBytes, long remaining)
    {
        var banks = new byte[8][];

        banks[5] = new byte[16384];
        stream.ReadExactly(banks[5]);

        banks[2] = new byte[16384];
        stream.ReadExactly(banks[2]);

        var pagedBankData = new byte[16384];
        stream.ReadExactly(pagedBankData);

        var footerData = new byte[4];
        stream.ReadExactly(footerData, 0, 4);

        var pagedBank = footerData[2] & 0x07;
        banks[pagedBank] = pagedBankData;

        var header = new SnaSnapshotHeader(headerBytes, footerData);

        foreach (var bankNumber in new byte[] { 0, 1, 3, 4, 6, 7 })
        {
            if (banks[bankNumber] != null)
            {
                continue;
            }

            banks[bankNumber] = new byte[16384];
            stream.ReadExactly(banks[bankNumber]);
        }

        // Ensure all banks are initialized.
        for (var i = 0; i < 8; i++)
        {
            banks[i] ??= new byte[16384];
        }

        return new SnaSnapshot128kFile(header, banks, footerData);
    }

    protected override void Write(SnaSnapshotFile file, Stream stream)
    {
        switch (file)
        {
            case SnaSnapshot48kFile file48K:
                Write48k(file48K, stream);
                break;
            case SnaSnapshot128kFile file128K:
                Write128k(file128K, stream);
                break;
        }
    }

    private static void Write48k(SnaSnapshot48kFile file, Stream stream)
    {
        // In 48K SNA files, PC is pushed onto the stack.
        var sp = file.Header.Registers.SP;
        sp -= 2;

        // Write the header with adjusted SP.
        var headerBytes = file.Header.AsReadOnlySpan().ToArray();
        headerBytes.SetWord(23, sp);
        stream.Write(headerBytes);

        // Write the RAM with PC at SP-2.
        var ram = file.Ram.ToArray();
        ram.SetWord(sp - 16384, file.Header.Registers.PC);
        stream.Write(ram);
    }

    private static void Write128k(SnaSnapshot128kFile file, Stream stream)
    {
        stream.Write(file.Header.AsReadOnlySpan());
        stream.Write(file.GetBank(5));
        stream.Write(file.GetBank(2));
        stream.Write(file.GetBank(file.PagedBank));

        var footer = new byte[4];
        footer.SetWord(0, file.Registers.PC);
        footer[2] = file.Port7FFD;
        footer[3] = file.TrDosRomPaged ? (byte)1 : (byte)0;
        stream.Write(footer);

        foreach (var bankNumber in new byte[] { 0, 1, 3, 4, 6, 7 })
        {
            if (bankNumber == file.PagedBank)
            {
                continue;
            }

            stream.Write(file.GetBank(bankNumber));
        }
    }
}