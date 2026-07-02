using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// The TAP tape file format for reading and writing TAP files.
/// </summary>
public sealed class TapFormat : ZXSpectrumTapeFormat<TapFile>
{
    /// <summary>
    /// The singleton instance of the TAP format.
    /// </summary>
    public static readonly TapFormat Instance = new();

    private TapFormat()
        : base("TAP Tape", "tap")
    {
    }

    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        var tapToTape = new TapToTapeConverter();
        yield return new TapToPzxConverter();
        yield return new TapToTzxConverter();
        yield return tapToTape;
        yield return new WavFileViaTapeConverter<TapFile>(Instance, tapToTape, new TapeToWavConverter(TStatesPerSecond));
    }

    /// <inheritdoc />
    [MustUseReturnValue]
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var blocks = new List<TapBlock>();

        while (!await reader.AtEndAsync().ConfigureAwait(false))
        {
            blocks.Add(await ReadBlockAsync(reader).ConfigureAwait(false));
        }

        return blocks.Count != 0 ? new TapFile(blocks) : throw new ArgumentException("Value was empty.", nameof(reader));
    }

    [MustUseReturnValue]
    private static async ValueTask<TapBlock> ReadBlockAsync(IBinaryReader reader)
    {
        var blockFlagAndChecksumLength = (await reader.ReadAsync(2).ConfigureAwait(false)).GetUInt16(0);

        // The block is the flag byte, the data and a trailing checksum byte.
        var content = await reader.ReadAsync(blockFlagAndChecksumLength).ConfigureAwait(false);
        var flag = content[0];
        var data = content[1..^1];

        var checksum = flag;
        foreach (var value in data)
        {
            checksum ^= value;
        }

        var trailer = new TapTrailer(content[^1]);
        if (checksum != trailer.Checksum)
        {
            throw new InvalidOperationException($"Expected TAP block to have checksum {trailer.Checksum} but found {checksum}.");
        }

        return (TapBlockType)flag switch
        {
            TapBlockType.Header => new HeaderBlock(new HeaderHeader(blockFlagAndChecksumLength), trailer, data),
            TapBlockType.Data => new DataBlock(new DataHeader(blockFlagAndChecksumLength), trailer, data),
            _ => throw new InvalidOperationException($"Unexpected TAP block type 0x{flag:X2}.")
        };
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(TapFile file, IBinaryWriter writer)
    {
        foreach (var block in file.Blocks)
        {
            await WriteBlockAsync(block, writer).ConfigureAwait(false);
        }
    }

    private static async ValueTask WriteBlockAsync(TapBlock block, IBinaryWriter writer)
    {
        await block.Header.WriteAsync(writer).ConfigureAwait(false);
        await block.WriteAsync(writer).ConfigureAwait(false);
        await block.Trailer.WriteAsync(writer).ConfigureAwait(false);
    }
}