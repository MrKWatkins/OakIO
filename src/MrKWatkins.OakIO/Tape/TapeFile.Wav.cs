using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Tape;

public sealed partial class TapeFile
{
    private const byte WavHighSignal = 0xC0;
    private const byte WavLowSignal = 0x40;

    [Pure]
    public WavFile ToWav(decimal tStatesPerSecond, uint sampleRateHz = 44100)
    {
        var tStatesPerSample = (int)Math.Round(tStatesPerSecond / sampleRateHz);

        Start();

        using var sampleData = new MemoryStream();

        while (!IsFinished)
        {
            var signal = Advance(tStatesPerSample);
            var value = signal ? WavHighSignal : WavLowSignal;
            sampleData.WriteByte(value);
        }

        return new WavFile(sampleRateHz, sampleData.ToArray());
    }
}
