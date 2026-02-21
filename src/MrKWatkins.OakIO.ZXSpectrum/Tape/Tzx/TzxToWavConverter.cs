using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Tape.Sounds;
using MrKWatkins.OakIO.Wav;
using OakTapeFile = MrKWatkins.OakIO.Tape.TapeFile;
using TapeDataBlock = MrKWatkins.OakIO.Tape.DataBlock;
using TapeLoopBlock = MrKWatkins.OakIO.Tape.LoopBlock;
using TapePauseBlock = MrKWatkins.OakIO.Tape.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class TzxToWavConverter(decimal tStatesPerSecond = 3_500_000m, uint sampleRateHz = 44100) : IFormatConverter<TzxFile, WavFile>
{
    [Pure]
    public WavFile Convert(TzxFile source)
    {
        var blocks = ConvertBlocks(source.Blocks).ToList();
        return new OakTapeFile(blocks).ToWav(tStatesPerSecond, sampleRateHz);
    }

    [Pure]
    private static IEnumerable<TapeBlock> ConvertBlocks(IReadOnlyList<TzxBlock> tzxBlocks)
    {
        var result = new List<TapeBlock>();
        var loopStartIndex = -1;
        var loopCount = 0;

        foreach (var tzxBlock in tzxBlocks)
        {
            switch (tzxBlock)
            {
                case LoopStartBlock loopStart:
                    loopStartIndex = result.Count;
                    loopCount = loopStart.Header.NumberOfRepetitions;
                    break;

                case LoopEndBlock:
                    var loopLength = result.Count - loopStartIndex;
                    var loopBlocks = result.Skip(loopStartIndex).ToList();
                    result.RemoveRange(loopStartIndex, loopLength);
                    if (loopCount > 0)
                        result.Add(new TapeLoopBlock(loopCount - 1, loopBlocks));
                    else
                        result.AddRange(loopBlocks);
                    loopStartIndex = -1;
                    break;

                default:
                    result.AddRange(ConvertBlock(tzxBlock));
                    break;
            }
        }

        return result;
    }

    [Pure]
    private static IEnumerable<TapeBlock> ConvertBlock(TzxBlock tzxBlock)
    {
        switch (tzxBlock)
        {
            case StandardSpeedDataBlock standardSpeed:
                var flagByte = standardSpeed.Length > 0 ? standardSpeed.Data[0] : (byte)0xFF;
                yield return new SoundBlock(flagByte == 0x00 ? Sound.StandardHeaderPureToneAndSync() : Sound.StandardDataPureToneAndSync());
                yield return TapeDataBlock.Create(standardSpeed.Data.ToArray());
                if (standardSpeed.Header.PauseAfterBlockMs > 0)
                    yield return new TapePauseBlock(standardSpeed.Header.PauseAfterBlockMs * 3500);
                break;

            case TurboSpeedDataBlock turboSpeed:
                var h = turboSpeed.Header;
                var usedBits = h.UsedBitsInLastByte == 0 ? 8 : h.UsedBitsInLastByte;
                yield return new SoundBlock(Sound.PureToneAndSync(h.PulsesInPilotTone, h.TStatesInPilotPulse, h.TStatesInSyncFirstPulse, h.TStatesInSyncSecondPulse));
                yield return TapeDataBlock.Create(turboSpeed.Data.ToArray(), Sound.Bit(h.TStatesInZeroBitPulse), Sound.Bit(h.TStatesInOneBitPulse), 0, usedBits);
                if (h.PauseAfterBlockMs > 0)
                    yield return new TapePauseBlock(h.PauseAfterBlockMs * 3500);
                break;

            case PureToneBlock pureTone:
                yield return new SoundBlock(Sound.PureTone(pureTone.Header.NumberOfPulses, pureTone.Header.LengthOfPulse));
                break;

            case PulseSequenceBlock pulseSeq:
                yield return new SoundBlock(Sound.PulseSequence(pulseSeq.Pulses));
                break;

            case PureDataBlock pureData:
                var pd = pureData.Header;
                var pureUsedBits = pd.UsedBitsInLastByte == 0 ? 8 : pd.UsedBitsInLastByte;
                yield return TapeDataBlock.Create(pureData.Data.ToArray(), Sound.Bit(pd.TStatesInZeroBitPulse), Sound.Bit(pd.TStatesInOneBitPulse), 0, pureUsedBits);
                if (pd.PauseAfterBlockMs > 0)
                    yield return new TapePauseBlock(pd.PauseAfterBlockMs * 3500);
                break;

            case PauseBlock pause:
                if (pause.Header.PauseMs > 0)
                    yield return new TapePauseBlock(pause.Header.PauseMs * 3500);
                break;
        }
    }
}