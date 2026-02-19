using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Tape.Sounds;
using MrKWatkins.OakIO.Wav;
using TapeDataBlock = MrKWatkins.OakIO.Tape.DataBlock;
using OakTapeFile = MrKWatkins.OakIO.Tape.TapeFile;
using TapePauseBlock = MrKWatkins.OakIO.Tape.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class PzxToWavConverter(decimal tStatesPerSecond = 3_500_000m, uint sampleRateHz = 44100) : IFormatConverter<PzxFile, WavFile>
{
    [Pure]
    public WavFile Convert(PzxFile source)
    {
        var blocks = source.Blocks.SelectMany(ConvertBlock).ToList();
        return new OakTapeFile(blocks).ToWav(tStatesPerSecond, sampleRateHz);
    }

    [Pure]
    private static IEnumerable<TapeBlock> ConvertBlock(PzxBlock pzxBlock)
    {
        switch (pzxBlock)
        {
            case PulseSequenceBlock puls:
                foreach (var pulse in puls.Pulses)
                    yield return new SoundBlock(Sound.PureTone(pulse.Count, (int)pulse.Duration));
                break;

            case DataBlock data:
                if (data.Header.SizeInBits == 0)
                    break;
                var zeroBitSound = data.ZeroBitPulseSequence.Length > 0
                    ? Sound.PulseSequence(data.ZeroBitPulseSequence)
                    : Sound.StandardZeroBit();
                var oneBitSound = data.OneBitPulseSequence.Length > 0
                    ? Sound.PulseSequence(data.OneBitPulseSequence)
                    : Sound.StandardOneBit();
                var usedBits = data.Header.ExtraBits > 0 ? (int)data.Header.ExtraBits : 8;
                bool? initialSignal = data.Header.InitialPulseLevel;
                yield return TapeDataBlock.Create(data.DataStream.ToArray(), zeroBitSound, oneBitSound, data.Header.Tail, usedBits, initialSignal);
                break;

            case PauseBlock pause:
                if (pause.Header.Duration > 0)
                    yield return new TapePauseBlock((int)pause.Header.Duration, pause.Header.InitialPulseLevel);
                break;
        }
    }
}