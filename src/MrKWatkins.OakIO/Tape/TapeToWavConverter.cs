using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Tape;

public sealed class TapeToWavConverter(decimal tStatesPerSecond) : WavFileConverter<TapeFile>(TapeFormat.Instance)
{
    private const byte WavHighSignal = 0xC0;
    private const byte WavLowSignal = 0x40;

    public override WavFile Convert(TapeFile source, uint sampleRateHz = IWavFileConverter.DefaultSampleRateHz)
    {
        var tStatesPerSample = (int)Math.Round(tStatesPerSecond / sampleRateHz);

        source.Start();

        using var sampleData = new MemoryStream();

        while (!source.IsFinished)
        {
            var signal = source.Advance(tStatesPerSample);
            sampleData.WriteByte(signal ? WavHighSignal : WavLowSignal);
        }

        return new WavFile(sampleRateHz, sampleData.ToArray());
    }
}