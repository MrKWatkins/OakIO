using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Tests.Wav;

public sealed class WavFileTests
{
    [Test]
    public void Constructor()
    {
        byte[] sampleData = [0x80, 0xC0, 0x40];
        var wavFile = new WavFile(44100, sampleData);

        wavFile.SampleRate.Should().Equal(44100u);
        wavFile.SampleData.Should().SequenceEqual(sampleData);
        wavFile.Format.Should().BeTheSameInstanceAs(WavFormat.Instance);
    }

    [Test]
    public void Constructor_Header()
    {
        byte[] sampleData = [0x80, 0xC0, 0x40];
        var wavFile = new WavFile(44100, sampleData);

        wavFile.Header.SampleRate.Should().Equal(44100u);
        wavFile.Header.DataSize.Should().Equal(sampleData.Length);
        wavFile.SampleRate.Should().Equal(wavFile.Header.SampleRate);
    }
}