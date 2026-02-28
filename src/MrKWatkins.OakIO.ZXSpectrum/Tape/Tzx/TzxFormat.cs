using MrKWatkins.BinaryPrimitives;
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
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        var tzxToTape = new TzxToTapeConverter();
        yield return new TzxToPzxConverter();
        yield return new TzxToTapConverter();
        yield return tzxToTape;
        yield return new WavFileViaTapeConverter<TzxFile>(Instance, tzxToTape, new TapeToWavConverter(TStatesPerSecond));
    }

    /// <inheritdoc />
    protected override TzxFile ReadTape(Stream stream)
    {
        var header = ReadHeader(stream);
        var blocks = ReadBlocks(stream).ToList();

        return new TzxFile(header, blocks);
    }

    private static IEnumerable<TzxBlock> ReadBlocks(Stream stream)
    {
        while (true)
        {
            var typeByte = stream.ReadByte();
            if (typeByte == -1)
            {
                yield break;
            }

            var type = (TzxBlockType)typeByte;
            yield return type switch
            {
                TzxBlockType.ArchiveInfo => new ArchiveInfoBlock(stream),
                TzxBlockType.GroupStart => new GroupStartBlock(stream),
                TzxBlockType.GroupEnd => new GroupEndBlock(stream),
                TzxBlockType.LoopStart => new LoopStartBlock(stream),
                TzxBlockType.LoopEnd => new LoopEndBlock(stream),
                TzxBlockType.Pause => new PauseBlock(stream),
                TzxBlockType.PulseSequence => new PulseSequenceBlock(stream),
                TzxBlockType.PureData => new PureDataBlock(stream),
                TzxBlockType.PureTone => new PureToneBlock(stream),
                TzxBlockType.StandardSpeedData => new StandardSpeedDataBlock(stream),
                TzxBlockType.StopTheTapeIf48K => new StopTheTapeIf48KBlock(stream),
                TzxBlockType.TextDescription => new TextDescriptionBlock(stream),
                TzxBlockType.TurboSpeedData => new TurboSpeedDataBlock(stream),
                _ => throw new NotSupportedException($"The block type {type} is not supported.")
            };
        }
    }

    [MustUseReturnValue]
    private static TzxHeader ReadHeader(Stream stream)
    {
        var bytes = stream.ReadExactly(TzxHeader.ExpectedLength);
        var header = new TzxHeader(bytes);
        return header.IsValid ? header : throw new IOException("Not a valid TZX file.");
    }

    /// <inheritdoc />
    protected override void Write(TzxFile file, Stream stream)
    {
        file.Header.Write(stream);
        foreach (var block in file.Blocks)
        {
            stream.WriteByte((byte)block.Header.Type);
            block.Header.Write(stream);
            block.Write(stream);
        }
    }
}