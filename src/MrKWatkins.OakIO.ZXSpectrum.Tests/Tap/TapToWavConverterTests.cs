using MrKWatkins.OakIO.Wav;
using MrKWatkins.OakIO.ZXSpectrum.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tap;

public sealed class TapToWavConverterTests
{
    [Test]
    public void Convert_ReturnsSamplesAtDefaultRate()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var wav = new TapToWavConverter().Convert(tap);

        wav.SampleRate.Should().Equal(44100u);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_CustomSampleRate()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var wav = new TapToWavConverter(sampleRateHz: 22050).Convert(tap);

        wav.SampleRate.Should().Equal(22050u);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_WavCanBeWrittenAndRead()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var wav = new TapToWavConverter().Convert(tap);

        using var stream = new MemoryStream();
        WavFormat.Instance.Write(wav, stream);
        stream.Position = 0;
        var readBack = WavFormat.Instance.Read(stream);
        readBack.SampleRate.Should().Equal(wav.SampleRate);
        readBack.SampleData.Should().HaveCount(wav.SampleData.Length);
    }

    [Test]
    public void Convert_SamplesContainBothHighAndLow()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);

        var wav = new TapToWavConverter().Convert(tap);

        wav.SampleData.Any(s => s == 0xC0).Should().BeTrue();
        wav.SampleData.Any(s => s == 0x40).Should().BeTrue();
    }
}
