using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Tape.Sounds;
using MrKWatkins.OakIO.Wav;
using OakTapeFile = MrKWatkins.OakIO.Tape.TapeFile;
using TapeDataBlock = MrKWatkins.OakIO.Tape.DataBlock;
using TapePauseBlock = MrKWatkins.OakIO.Tape.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tap;

public sealed class TapToWavConverter(decimal tStatesPerSecond = 3_500_000m, uint sampleRateHz = 44100) : IFormatConverter<TapFile, WavFile>
{
    [Pure]
    public WavFile Convert(TapFile source)
    {
        var blocks = source.Blocks.SelectMany(ConvertBlock).ToList();
        return new OakTapeFile(blocks).ToWav(tStatesPerSecond, sampleRateHz);
    }

    [Pure]
    private static IEnumerable<TapeBlock> ConvertBlock(TapBlock block)
    {
        var isHeader = block.Header.Type == TapBlockType.Header;
        yield return new SoundBlock(isHeader ? Sound.StandardHeaderPureToneAndSync() : Sound.StandardDataPureToneAndSync());

        var blockData = BuildBlockData(block);
        yield return TapeDataBlock.Create(blockData);

        yield return new TapePauseBlock(3_500_000);
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