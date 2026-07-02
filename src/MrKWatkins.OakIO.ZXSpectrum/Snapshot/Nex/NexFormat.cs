using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// The NEX snapshot file format for the ZX Spectrum Next.
/// </summary>
public sealed class NexFormat : ZXSpectrumSnapshotFormat<NexFile>
{
    /// <summary>
    /// The singleton instance of the NEX format.
    /// </summary>
    public static readonly NexFormat Instance = new();

    private NexFormat()
        : base("NEX", "nex")
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var header = new NexHeader(await reader.ReadAsync(NexHeader.Size).ConfigureAwait(false));

        if (header.Magic != "Next")
        {
            throw new InvalidOperationException("Not a valid NEX file; expected magic value \"Next\".");
        }

        var palette = await ReadPaletteAsync(reader, header).ConfigureAwait(false);
        var screens = await ReadScreensAsync(reader, header).ConfigureAwait(false);
        var copperCode = await ReadCopperCodeAsync(reader, header).ConfigureAwait(false);
        var banks = await ReadBanksAsync(reader, header).ConfigureAwait(false);

        return new NexFile(header, palette, screens, copperCode, banks);
    }

    [MustUseReturnValue]
    private static async ValueTask<byte[]?> ReadPaletteAsync(IBinaryReader reader, NexHeader header)
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

        return await reader.ReadAsync(512).ConfigureAwait(false);
    }

    [MustUseReturnValue]
    private static async ValueTask<List<NexScreen>> ReadScreensAsync(IBinaryReader reader, NexHeader header)
    {
        var screens = new List<NexScreen>();
        var loadScreens = header.LoadScreensByte;

        if ((loadScreens & 0b0000_0001) != 0)
        {
            screens.Add(await ReadScreenAsync(reader, NexScreenType.Layer2, 49152).ConfigureAwait(false));
        }

        if ((loadScreens & 0b0000_0010) != 0)
        {
            screens.Add(await ReadScreenAsync(reader, NexScreenType.Ula, 6912).ConfigureAwait(false));
        }

        if ((loadScreens & 0b0000_0100) != 0)
        {
            screens.Add(await ReadScreenAsync(reader, NexScreenType.LoRes, 12288).ConfigureAwait(false));
        }

        if ((loadScreens & 0b0000_1000) != 0)
        {
            screens.Add(await ReadScreenAsync(reader, NexScreenType.HiRes, 12288).ConfigureAwait(false));
        }

        if ((loadScreens & 0b0001_0000) != 0)
        {
            screens.Add(await ReadScreenAsync(reader, NexScreenType.HiColour, 12288).ConfigureAwait(false));
        }

        if ((loadScreens & 0b0100_0000) != 0)
        {
            var screenType = header.LoadScreens2 switch
            {
                NexLoadScreenMode.Layer2x320x256 => NexScreenType.Layer2x320x256,
                NexLoadScreenMode.Layer2x640x256 => NexScreenType.Layer2x640x256,
                _ => NexScreenType.Layer2x320x256
            };
            screens.Add(await ReadScreenAsync(reader, screenType, 81920).ConfigureAwait(false));
        }

        return screens;
    }

    [MustUseReturnValue]
    private static async ValueTask<NexScreen> ReadScreenAsync(IBinaryReader reader, NexScreenType type, int size) =>
        new(type, await reader.ReadAsync(size).ConfigureAwait(false));

    [MustUseReturnValue]
    private static async ValueTask<byte[]?> ReadCopperCodeAsync(IBinaryReader reader, NexHeader header)
    {
        if (header.Version < NexVersion.V13 || !header.HasCopperCode)
        {
            return null;
        }

        return await reader.ReadAsync(2048).ConfigureAwait(false);
    }

    [MustUseReturnValue]
    private static async ValueTask<List<NexBank>> ReadBanksAsync(IBinaryReader reader, NexHeader header)
    {
        var banks = new List<NexBank>();

        foreach (var bank in NexHeader.BankOrder)
        {
            if (!header.IsBankIncluded(bank))
            {
                continue;
            }

            banks.Add(new NexBank(bank, await reader.ReadAsync(16384).ConfigureAwait(false)));
        }

        return banks;
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(NexFile file, IBinaryWriter writer)
    {
        await file.Header.WriteAsync(writer).ConfigureAwait(false);

        if (file.Palette != null)
        {
            await writer.WriteAsync(file.Palette).ConfigureAwait(false);
        }

        foreach (var screen in file.Screens)
        {
            await writer.WriteAsync(screen.Data).ConfigureAwait(false);
        }

        if (file.CopperCode != null)
        {
            await writer.WriteAsync(file.CopperCode).ConfigureAwait(false);
        }

        var banksByNumber = file.Banks.ToDictionary(b => b.BankNumber);

        foreach (var bankNumber in NexHeader.BankOrder)
        {
            if (banksByNumber.TryGetValue(bankNumber, out var nexBank))
            {
                await writer.WriteAsync(nexBank.Data).ConfigureAwait(false);
            }
        }
    }
}