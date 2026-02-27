using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Tape.Sounds;
using OakTapeFile = MrKWatkins.OakIO.Tape.TapeFile;
using TapeDataBlock = MrKWatkins.OakIO.Tape.DataBlock;
using TapePauseBlock = MrKWatkins.OakIO.Tape.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Converts TAP files to the generic tape format.
/// </summary>
public sealed class TapToTapeConverter() : IOFileConverter<TapFile, OakTapeFile>(TapFormat.Instance, TapeFormat.Instance)
{
    /// <inheritdoc />
    public override OakTapeFile Convert(TapFile source)
    {
        var blocks = source.Blocks.SelectMany(ConvertBlock).ToList();
        return new OakTapeFile(blocks);
    }

    [Pure]
    private IEnumerable<TapeBlock> ConvertBlock(TapBlock block)
    {
        var isHeader = block.Header.Type == TapBlockType.Header;
        yield return new SoundBlock(isHeader ? Sound.StandardHeaderPureToneAndSync() : Sound.StandardDataPureToneAndSync());

        var blockData = BuildBlockData(block);
        yield return TapeDataBlock.Create(blockData);

        yield return new TapePauseBlock((int)ZXSpectrumTapeFormat.TStatesPerSecond);
    }

    [Pure]
    private static byte[] BuildBlockData(TapBlock block)
    {
        var data = new byte[1 + block.Length + 1];
        data[0] = (byte)block.Header.Type;
        block.CopyTo(data.AsSpan(1));
        data[^1] = block.Trailer.Checksum;
        return data;
    }
}