using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Tape.Sounds;
using OakTapeFile = MrKWatkins.OakIO.Tape.TapeFile;
using TapeDataBlock = MrKWatkins.OakIO.Tape.DataBlock;
using TapeLoopBlock = MrKWatkins.OakIO.Tape.LoopBlock;
using TapePauseBlock = MrKWatkins.OakIO.Tape.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class TzxToTapeConverter() : IOFileConverter<TzxFile, OakTapeFile>(TzxFormat.Instance, TapeFormat.Instance)
{
    public override OakTapeFile Convert(TzxFile source)
    {
        var blocks = ConvertBlocks(source.Blocks);
        return new OakTapeFile(blocks);
    }

    [Pure]
    private static List<TapeBlock> ConvertBlocks(IReadOnlyList<TzxBlock> tzxBlocks)
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
                    {
                        result.Add(new TapeLoopBlock(loopCount - 1, loopBlocks));
                    }
                    else
                    {
                        result.AddRange(loopBlocks);
                    }
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
                {
                    yield return new TapePauseBlock(standardSpeed.Header.PauseAfterBlockMs * 3500);
                }
                break;

            case TurboSpeedDataBlock turboSpeed:
                var turboSpeedHeader = turboSpeed.Header;
                var usedBits = turboSpeedHeader.UsedBitsInLastByte == 0 ? 8 : turboSpeedHeader.UsedBitsInLastByte;
                yield return new SoundBlock(Sound.PureToneAndSync(turboSpeedHeader.PulsesInPilotTone, turboSpeedHeader.TStatesInPilotPulse, turboSpeedHeader.TStatesInSyncFirstPulse, turboSpeedHeader.TStatesInSyncSecondPulse));
                yield return TapeDataBlock.Create(turboSpeed.Data, Sound.Bit(turboSpeedHeader.TStatesInZeroBitPulse), Sound.Bit(turboSpeedHeader.TStatesInOneBitPulse), 0, usedBits);
                if (turboSpeedHeader.PauseAfterBlockMs > 0)
                {
                    yield return new TapePauseBlock(turboSpeedHeader.PauseAfterBlockMs * 3500);
                }
                break;

            case PureToneBlock pureTone:
                yield return new SoundBlock(Sound.PureTone(pureTone.Header.NumberOfPulses, pureTone.Header.LengthOfPulse));
                break;

            case PulseSequenceBlock pulseSeq:
                yield return new SoundBlock(Sound.PulseSequence(pulseSeq.Pulses));
                break;

            case PureDataBlock pureData:
                var pureDataHeader = pureData.Header;
                var pureUsedBits = pureDataHeader.UsedBitsInLastByte == 0 ? 8 : pureDataHeader.UsedBitsInLastByte;
                yield return TapeDataBlock.Create(pureData.Data.ToArray(), Sound.Bit(pureDataHeader.TStatesInZeroBitPulse), Sound.Bit(pureDataHeader.TStatesInOneBitPulse), 0, pureUsedBits);
                if (pureDataHeader.PauseAfterBlockMs > 0)
                {
                    yield return new TapePauseBlock(pureDataHeader.PauseAfterBlockMs * 3500);
                }
                break;

            case PauseBlock pause:
                if (pause.Header.PauseMs > 0)
                {
                    yield return new TapePauseBlock(pause.Header.PauseMs * 3500);
                }
                break;
        }
    }
}