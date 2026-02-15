namespace MrKWatkins.OakIO.Wav;

public sealed class WavFile(uint sampleRate, byte[] sampleData) : IOFile(WavFormat.Instance)
{
    public uint SampleRate { get; } = sampleRate;

    public byte[] SampleData { get; } = sampleData;
}
