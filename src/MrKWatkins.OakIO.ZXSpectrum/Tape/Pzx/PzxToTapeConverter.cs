using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Tape.Sounds;
using TapeDataBlock = MrKWatkins.OakIO.Tape.DataBlock;
using OakTapeFile = MrKWatkins.OakIO.Tape.TapeFile;
using TapePauseBlock = MrKWatkins.OakIO.Tape.PauseBlock;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// Converts a <see cref="PzxFile" /> to a <see cref="OakTapeFile" />.
/// </summary>
public sealed class PzxToTapeConverter() : IOFileConverter<PzxFile, OakTapeFile>(PzxFormat.Instance, TapeFormat.Instance)
{
    /// <inheritdoc />
    public override OakTapeFile Convert(PzxFile source)
    {
        var blocks = source.Blocks.SelectMany(ConvertBlock).ToList();
        return new OakTapeFile(blocks);
    }

    [Pure]
    private static IEnumerable<TapeBlock> ConvertBlock(PzxBlock pzxBlock)
    {
        switch (pzxBlock)
        {
            case PulseSequenceBlock pulseSequence:
                // From https://github.com/raxoft/pzxtools/blob/master/docs/pzx_format.txt:
                // "The pulse level is low at start of the block by default."
                var firstPulse = true;
                foreach (var pulse in pulseSequence.Pulses)
                {
                    yield return new SoundBlock(Sound.PureTone(pulse.Count, (int)pulse.Duration), firstPulse ? false : null);
                    firstPulse = false;
                }
                break;

            case DataBlock data:
                if (data.Header.SizeInBits == 0)
                {
                    break;
                }
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
                {
                    yield return new TapePauseBlock((int)pause.Header.Duration, pause.Header.InitialPulseLevel);
                }
                break;
        }
    }
}