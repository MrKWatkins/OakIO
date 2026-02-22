namespace MrKWatkins.OakIO.Wav;

public interface IWavFileConverter
{
    public const uint DefaultSampleRateHz = 44100;

    [Pure]
    WavFile Convert(IOFile source, uint sampleRateHz = DefaultSampleRateHz);
}