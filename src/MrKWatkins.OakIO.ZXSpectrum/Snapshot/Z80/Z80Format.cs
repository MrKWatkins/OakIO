using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// File format for Z80 snapshot files.
/// </summary>
// https://worldofspectrum.org/faq/reference/z80format.htm
public sealed class Z80Format : ZXSpectrumSnapshotFormat<Z80File>
{
    /// <summary>
    /// The singleton instance of the Z80 format.
    /// </summary>
    public static readonly Z80Format Instance = new();

    private Z80Format()
        : base("Z80 Snapshot", "z80")
    {
    }

    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        yield return new Z80ToSnaConverter();
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var v1HeaderBytes = await reader.ReadAsync(30).ConfigureAwait(false);
        return v1HeaderBytes.GetUInt16(6) != 0
            ? await ReadV1Async(reader, v1HeaderBytes).ConfigureAwait(false)
            : await ReadV2OrV3Async(reader, v1HeaderBytes).ConfigureAwait(false);
    }

    [MustUseReturnValue]
    private static async ValueTask<Z80File> ReadV1Async(IBinaryReader reader, byte[] v1HeaderBytes)
    {
        var header = new Z80V1Header(v1HeaderBytes);
        var data = await reader.ReadToEndAsync().ConfigureAwait(false);

        return new Z80V1File(header, data);
    }

    [MustUseReturnValue]
    private static async ValueTask<Z80File> ReadV2OrV3Async(IBinaryReader reader, byte[] v1HeaderBytes)
    {
        var extraLength = (await reader.ReadAsync(2).ConfigureAwait(false)).GetUInt16(0);

        // Extra length does not include the 2 bytes for the extraLength word.
        var headerBytes = new byte[30 + 2 + extraLength];
        v1HeaderBytes.CopyTo(headerBytes, 0);
        headerBytes.SetUInt16(30, extraLength);
        (await reader.ReadAsync(extraLength).ConfigureAwait(false)).CopyTo(headerBytes, 32);

        switch (extraLength)
        {
            case 23:
                {
                    var header = new Z80V2Header(headerBytes);
                    return new Z80V2File(header, await LoadPagesAsync(header.HardwareMode, reader).ConfigureAwait(false));
                }
            case 54 or 55:
                {
                    var header = new Z80V3Header(headerBytes);
                    return new Z80V3File(header, await LoadPagesAsync(header.HardwareMode, reader).ConfigureAwait(false));
                }
        }

        throw new InvalidOperationException($"An extra header length of {extraLength} does not correspond to a known Z80 version.");
    }

    [MustUseReturnValue]
    private static async ValueTask<List<Page>> LoadPagesAsync(HardwareMode hardwareMode, IBinaryReader reader)
    {
        var pages = new List<Page>();
        while (!await reader.AtEndAsync().ConfigureAwait(false))
        {
            pages.Add(await LoadPageAsync(hardwareMode, reader).ConfigureAwait(false));
        }

        return pages;
    }

    [MustUseReturnValue]
    private static async ValueTask<Page> LoadPageAsync(HardwareMode hardwareMode, IBinaryReader reader)
    {
        var headerBytes = await reader.ReadAsync(3).ConfigureAwait(false);

        var header = new PageHeader(hardwareMode, headerBytes);

        var length = header.CompressedLength == 0xFFFF ? 16384 : header.CompressedLength;
        var data = await reader.ReadAsync(length).ConfigureAwait(false);

        return new Page(header, data);
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(Z80File file, IBinaryWriter writer)
    {
        if (file is Z80V1File && file.Registers.PC == 0)
        {
            throw new InvalidOperationException("PC cannot be 0 for a v1 file; a PC value of 0 is to specify a v2 or v3 file.");
        }

        await file.Header.WriteAsync(writer).ConfigureAwait(false);
        if (file is Z80V1File v1File)
        {
            await writer.WriteAsync(v1File.CompressedDataMemory).ConfigureAwait(false);
        }
        else
        {
            await WriteV2OrV3DataAsync((IZ80SnapshotV2OrV3File)file, writer).ConfigureAwait(false);
        }
    }

    private static async ValueTask WriteV2OrV3DataAsync(IZ80SnapshotV2OrV3File v2File, IBinaryWriter writer)
    {
        foreach (var page in v2File.Pages)
        {
            await page.Header.WriteAsync(writer).ConfigureAwait(false);
            await page.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}