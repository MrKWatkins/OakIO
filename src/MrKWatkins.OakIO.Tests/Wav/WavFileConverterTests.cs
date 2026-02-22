using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Tests.Wav;

public sealed class WavFileConverterTests
{
    [Test]
    public void SourceFormat()
    {
        var converter = new TapeToWavConverter(100m);
        converter.SourceFormat.Should().BeTheSameInstanceAs(TapeFormat.Instance);
    }

    [Test]
    public void TargetFormat()
    {
        var converter = new TapeToWavConverter(100m);
        converter.TargetFormat.Should().BeTheSameInstanceAs(WavFormat.Instance);
    }

    [Test]
    public void Convert_TSource()
    {
        // Use tStatesPerSecond matching the default sample rate so tStatesPerSample = 1.
        // Cast to IOFileConverter to avoid the compiler preferring Convert(TapeFile, uint) directly.
        var tape = new TapeFile([new PauseBlock(100, initialSignal: true)]);
        IOFileConverter<TapeFile, WavFile> converter = new TapeToWavConverter(IWavFileConverter.DefaultSampleRateHz);

        var wav = converter.Convert(tape);

        wav.SampleRate.Should().Equal(IWavFileConverter.DefaultSampleRateHz);
        wav.SampleData.Should().NotBeEmpty();
    }
}