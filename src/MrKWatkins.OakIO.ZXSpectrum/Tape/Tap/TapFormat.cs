using MrKWatkins.BinaryPrimitives;
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
    protected override IEnumerable<IOFileConverter> CreateConverters()
    {
        var tapToTape = new TapToTapeConverter();
        yield return new TapToPzxConverter();
        yield return new TapToTzxConverter();
        yield return tapToTape;
        yield return new WavFileViaTapeConverter<TapFile>(Instance, tapToTape, new TapeToWavConverter(TStatesPerSecond));
    }

    /// <inheritdoc />
    protected override TapFile ReadTape(Stream stream)
    {
        using var peekable = new PeekableStream(stream);

        var blocks = new List<TapBlock>();

        while (!peekable.EndOfStream)
        {
            blocks.Add(ReadBlock(peekable));
        }

        return blocks.Count != 0 ? new TapFile(blocks) : throw new ArgumentException("Value was empty.", nameof(stream));
    }

    [MustUseReturnValue]
    private static TapBlock ReadBlock(Stream stream)
    {
        var blockFlagAndChecksumLength = stream.ReadUInt16OrThrow();
        var flag = stream.ReadByteOrThrow();

        var data = new byte[blockFlagAndChecksumLength - 2];

        var checksum = flag;
        for (var f = 0; f < data.Length; f++)
        {
            data[f] = stream.ReadByteOrThrow();

            checksum ^= data[f];
        }

        var trailer = new TapTrailer(stream.ReadByteOrThrow());
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
    protected override void Write(TapFile file, Stream stream)
    {
        foreach (var block in file.Blocks)
        {
            WriteBlock(block, stream);
        }
    }

    private static void WriteBlock(TapBlock block, Stream stream)
    {
        block.Header.Write(stream);
        block.Write(stream);
        block.Trailer.Write(stream);
    }
}