namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

public sealed class NexFormat : SnapshotFormat<NexFile>
{
    public static readonly NexFormat Instance = new();

    private NexFormat()
        : base("NEX", "nex")
    {
    }

    protected override NexFile ReadSnapshot(Stream stream)
    {
        var headerBytes = new byte[NexHeader.Size];
        stream.ReadExactly(headerBytes);

        var header = new NexHeader(headerBytes);

        if (header.Magic != "Next")
        {
            throw new InvalidOperationException("Not a valid NEX file; expected magic value \"Next\".");
        }

        var palette = ReadPalette(stream, header);
        var screens = ReadScreens(stream, header);
        var copperCode = ReadCopperCode(stream, header);
        var banks = ReadBanks(stream, header);

        return new NexFile(header, palette, screens, copperCode, banks);
    }

    [MustUseReturnValue]
    private static byte[]? ReadPalette(Stream stream, NexHeader header)
    {
        if (header.HasNoPaletteBlock)
        {
            return null;
        }

        var loadScreens = header.LoadScreensByte;

        var hasLayer2 = (loadScreens & 0b0000_0001) != 0;
        var hasLoRes = (loadScreens & 0b0000_0100) != 0;
        var hasFlags2 = (loadScreens & 0b0100_0000) != 0;
        var isTilemode = hasFlags2 && header.LoadScreens2 == NexLoadScreenMode.Tilemode;

        if (!hasLayer2 && !hasLoRes && !isTilemode)
        {
            return null;
        }

        var palette = new byte[512];
        stream.ReadExactly(palette);
        return palette;
    }

    [MustUseReturnValue]
    private static List<NexScreen> ReadScreens(Stream stream, NexHeader header)
    {
        var screens = new List<NexScreen>();
        var loadScreens = header.LoadScreensByte;

        if ((loadScreens & 0b0000_0001) != 0)
        {
            screens.Add(ReadScreen(stream, NexScreenType.Layer2, 49152));
        }

        if ((loadScreens & 0b0000_0010) != 0)
        {
            screens.Add(ReadScreen(stream, NexScreenType.Ula, 6912));
        }

        if ((loadScreens & 0b0000_0100) != 0)
        {
            screens.Add(ReadScreen(stream, NexScreenType.LoRes, 12288));
        }

        if ((loadScreens & 0b0000_1000) != 0)
        {
            screens.Add(ReadScreen(stream, NexScreenType.HiRes, 12288));
        }

        if ((loadScreens & 0b0001_0000) != 0)
        {
            screens.Add(ReadScreen(stream, NexScreenType.HiColour, 12288));
        }

        if ((loadScreens & 0b0100_0000) != 0)
        {
            var screenType = header.LoadScreens2 switch
            {
                NexLoadScreenMode.Layer2x320x256 => NexScreenType.Layer2x320x256,
                NexLoadScreenMode.Layer2x640x256 => NexScreenType.Layer2x640x256,
                _ => NexScreenType.Layer2x320x256
            };
            screens.Add(ReadScreen(stream, screenType, 81920));
        }

        return screens;
    }

    [MustUseReturnValue]
    private static NexScreen ReadScreen(Stream stream, NexScreenType type, int size)
    {
        var data = new byte[size];
        stream.ReadExactly(data);
        return new NexScreen(type, data);
    }

    [MustUseReturnValue]
    private static byte[]? ReadCopperCode(Stream stream, NexHeader header)
    {
        if (header.Version < NexVersion.V13 || !header.HasCopperCode)
        {
            return null;
        }

        var data = new byte[2048];
        stream.ReadExactly(data);
        return data;
    }

    [MustUseReturnValue]
    private static List<NexBank> ReadBanks(Stream stream, NexHeader header)
    {
        var banks = new List<NexBank>();

        foreach (var bank in NexHeader.BankOrder)
        {
            if (!header.IsBankIncluded(bank))
            {
                continue;
            }

            var data = new byte[16384];
            stream.ReadExactly(data);
            banks.Add(new NexBank(bank, data));
        }

        return banks;
    }

    protected override void Write(NexFile file, Stream stream)
    {
        stream.Write(file.Header.AsReadOnlySpan());

        if (file.Palette != null)
        {
            stream.Write(file.Palette);
        }

        foreach (var screen in file.Screens)
        {
            stream.Write(screen.Data);
        }

        if (file.CopperCode != null)
        {
            stream.Write(file.CopperCode);
        }

        var banksByNumber = file.Banks.ToDictionary(b => b.BankNumber);

        foreach (var bankNumber in NexHeader.BankOrder)
        {
            if (banksByNumber.TryGetValue(bankNumber, out var nexBank))
            {
                stream.Write(nexBank.Data);
            }
        }
    }
}