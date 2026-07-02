using System.Text;
using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The PZX tape image file format.
/// </summary>
// https://github.com/raxoft/pzxtools/blob/master/docs/pzx_format.txt
public sealed class PzxFormat : ZXSpectrumTapeFormat<PzxFile>
{
    /// <summary>
    /// The singleton instance of the PZX format.
    /// </summary>
    public static readonly PzxFormat Instance = new();

    private PzxFormat()
        : base("PZX Tape", "pzx")
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        var pzxToTape = new PzxToTapeConverter();
        yield return new PzxToTzxConverter();
        yield return new PzxToTapConverter();
        yield return pzxToTape;
        yield return new WavFileViaTapeConverter<PzxFile>(Instance, pzxToTape, new TapeToWavConverter(TStatesPerSecond));
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var blocks = new List<PzxBlock>();

        while (!await reader.AtEndAsync().ConfigureAwait(false))
        {
            blocks.Add(await ReadBlockAsync(reader).ConfigureAwait(false));
        }

        return new PzxFile(blocks);
    }

    [MustUseReturnValue]
    private static async ValueTask<PzxBlock> ReadBlockAsync(IBinaryReader reader)
    {
        var typeBytes = await reader.ReadAsync(4).ConfigureAwait(false);
        var type = (PzxBlockType)typeBytes.GetUInt32(0, Endian.Big);

        switch (type)
        {
            case PzxBlockType.Header:
                var (pzxHeader, pzxBody) = await ReadHeaderAndBodyAsync(reader, 6).ConfigureAwait(false);
                return new PzxHeaderBlock(pzxHeader, pzxBody);
            case PzxBlockType.PulseSequence:
                var (pulseHeader, pulseBody) = await ReadHeaderAndBodyAsync(reader, 4).ConfigureAwait(false);
                return new PulseSequenceBlock(pulseHeader, pulseBody);
            case PzxBlockType.Data:
                var (dataHeader, dataBody) = await ReadHeaderAndBodyAsync(reader, 12).ConfigureAwait(false);
                return new DataBlock(dataHeader, dataBody);
            case PzxBlockType.Pause:
                var (pauseHeader, _) = await ReadHeaderAndBodyAsync(reader, 8).ConfigureAwait(false);
                return new PauseBlock(pauseHeader);
            case PzxBlockType.BrowsePoint:
                var (browseHeader, browseBody) = await ReadHeaderAndBodyAsync(reader, 4).ConfigureAwait(false);
                return new BrowsePointBlock(browseHeader, browseBody);
            case PzxBlockType.Stop:
                var (stopHeader, _) = await ReadHeaderAndBodyAsync(reader, 6).ConfigureAwait(false);
                return new StopBlock(stopHeader);
            default:
                throw new NotSupportedException($"The block type {Encoding.ASCII.GetString(typeBytes)} is not supported.");
        }
    }

    [MustUseReturnValue]
    private static async ValueTask<(byte[] Header, byte[] Body)> ReadHeaderAndBodyAsync(IBinaryReader reader, int headerSize)
    {
        var header = await reader.ReadAsync(headerSize).ConfigureAwait(false);

        // The size field at the start of the header covers the header fields (after the size field) plus the body.
        var bodyLength = (int)header.GetUInt32(0) - (headerSize - 4);
        var body = await reader.ReadAsync(bodyLength).ConfigureAwait(false);

        return (header, body);
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(PzxFile file, IBinaryWriter writer)
    {
        // Reused for the four-byte type tag written before each block; safe as each write is awaited before the next.
        var typeTag = new byte[4];
        foreach (var block in file.Blocks)
        {
            typeTag.SetUInt32(0, (uint)block.Header.Type, Endian.Big);
            await writer.WriteAsync(typeTag).ConfigureAwait(false);
            await block.Header.WriteAsync(writer).ConfigureAwait(false);
            await block.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}