using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

// https://worldofspectrum.net/TZXformat.html
// https://worldofspectrum.net/zx-modules/fileformats/tzxformat.html
/// <summary>
/// The TZX tape file format for the ZX Spectrum.
/// </summary>
public sealed class TzxFormat : ZXSpectrumTapeFormat<TzxFile>
{
    /// <summary>
    /// Gets the singleton instance of the TZX format.
    /// </summary>
    public static readonly TzxFormat Instance = new();

    private TzxFormat()
        : base("TZX Tape", "tzx")
    {
    }

    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        var tzxToTape = new TzxToTapeConverter();
        yield return new TzxToPzxConverter();
        yield return new TzxToTapConverter();
        yield return tzxToTape;
        yield return new WavFileViaTapeConverter<TzxFile>(Instance, tzxToTape, new TapeToWavConverter(TStatesPerSecond));
    }

    /// <inheritdoc />
    [MustUseReturnValue]
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader)
    {
        var header = new TzxHeader(await reader.ReadAsync(TzxHeader.ExpectedLength).ConfigureAwait(false));
        if (!header.IsValid)
        {
            throw new IOException("Not a valid TZX file.");
        }

        var blocks = new List<TzxBlock>();
        while (!await reader.AtEndAsync().ConfigureAwait(false))
        {
            blocks.Add(await ReadBlockAsync(reader).ConfigureAwait(false));
        }

        return new TzxFile(header, blocks);
    }

    [MustUseReturnValue]
    private static async ValueTask<TzxBlock> ReadBlockAsync(IBinaryReader reader)
    {
        var type = (TzxBlockType)(await reader.ReadAsync(1).ConfigureAwait(false))[0];

        switch (type)
        {
            case TzxBlockType.ArchiveInfo:
                var (archiveHeader, archiveBody) = await ReadHeaderAndBodyAsync(reader, 3, d => new ArchiveInfoHeader(d).BlockLength).ConfigureAwait(false);
                return new ArchiveInfoBlock(archiveHeader, archiveBody);
            case TzxBlockType.GroupStart:
                var (groupStartHeader, groupStartBody) = await ReadHeaderAndBodyAsync(reader, 1, d => new GroupStartHeader(d).BlockLength).ConfigureAwait(false);
                return new GroupStartBlock(groupStartHeader, groupStartBody);
            case TzxBlockType.GroupEnd:
                return new GroupEndBlock(await reader.ReadAsync(0).ConfigureAwait(false));
            case TzxBlockType.LoopStart:
                return new LoopStartBlock(await reader.ReadAsync(2).ConfigureAwait(false));
            case TzxBlockType.LoopEnd:
                return new LoopEndBlock(await reader.ReadAsync(0).ConfigureAwait(false));
            case TzxBlockType.Pause:
                return new PauseBlock(await reader.ReadAsync(2).ConfigureAwait(false));
            case TzxBlockType.PulseSequence:
                var (pulseHeader, pulseBody) = await ReadHeaderAndBodyAsync(reader, 1, d => new PulseSequenceHeader(d).BlockLength).ConfigureAwait(false);
                return new PulseSequenceBlock(pulseHeader, pulseBody);
            case TzxBlockType.PureData:
                var (pureDataHeader, pureDataBody) = await ReadHeaderAndBodyAsync(reader, 10, d => new PureDataHeader(d).BlockLength).ConfigureAwait(false);
                return new PureDataBlock(pureDataHeader, pureDataBody);
            case TzxBlockType.PureTone:
                return new PureToneBlock(await reader.ReadAsync(4).ConfigureAwait(false));
            case TzxBlockType.StandardSpeedData:
                var (standardHeader, standardBody) = await ReadHeaderAndBodyAsync(reader, 4, d => new StandardSpeedDataHeader(d).BlockLength).ConfigureAwait(false);
                return new StandardSpeedDataBlock(standardHeader, standardBody);
            case TzxBlockType.StopTheTapeIf48K:
                return new StopTheTapeIf48KBlock(await reader.ReadAsync(4).ConfigureAwait(false));
            case TzxBlockType.TextDescription:
                var (textHeader, textBody) = await ReadHeaderAndBodyAsync(reader, 1, d => new TextDescriptionHeader(d).BlockLength).ConfigureAwait(false);
                return new TextDescriptionBlock(textHeader, textBody);
            case TzxBlockType.TurboSpeedData:
                var (turboHeader, turboBody) = await ReadHeaderAndBodyAsync(reader, 18, d => new TurboSpeedDataHeader(d).BlockLength).ConfigureAwait(false);
                return new TurboSpeedDataBlock(turboHeader, turboBody);
            default:
                throw new NotSupportedException($"The block type {type} is not supported.");
        }
    }

    [MustUseReturnValue]
    private static async ValueTask<(byte[] Header, byte[] Body)> ReadHeaderAndBodyAsync(IBinaryReader reader, int headerSize, Func<byte[], int> blockLength)
    {
        var header = await reader.ReadAsync(headerSize).ConfigureAwait(false);
        var body = await reader.ReadAsync(blockLength(header)).ConfigureAwait(false);

        return (header, body);
    }

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(TzxFile file, IBinaryWriter writer)
    {
        await file.Header.WriteAsync(writer).ConfigureAwait(false);

        // Reused for the one-byte type tag written before each block; safe as each write is awaited before the next.
        var typeTag = new byte[1];
        foreach (var block in file.Blocks)
        {
            typeTag[0] = (byte)block.Header.Type;
            await writer.WriteAsync(typeTag).ConfigureAwait(false);
            await block.Header.WriteAsync(writer).ConfigureAwait(false);
            await block.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}