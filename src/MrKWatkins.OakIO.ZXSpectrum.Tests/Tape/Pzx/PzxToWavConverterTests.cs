using MrKWatkins.OakIO.Wav;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Pzx;

public sealed class PzxToWavConverterTests
{
    [Test]
    public void Convert_ReturnsSamplesAtDefaultRate()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var wav = new PzxToWavConverter().Convert(pzx);

        wav.SampleRate.Should().Equal(44100u);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_CustomSampleRate()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var wav = new PzxToWavConverter(sampleRateHz: 22050).Convert(pzx);

        wav.SampleRate.Should().Equal(22050u);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_SamplesContainBothHighAndLow()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var wav = new PzxToWavConverter().Convert(pzx);

        wav.SampleData.Any(s => s == 0xC0).Should().BeTrue();
        wav.SampleData.Any(s => s == 0x40).Should().BeTrue();
    }

    [Test]
    public void Convert_WavCanBeWrittenAndRead()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var pzx = TapToPzxConverter.Instance.Convert(tap);

        var wav = new PzxToWavConverter().Convert(pzx);

        using var stream = new MemoryStream();
        WavFormat.Instance.Write(wav, stream);
        stream.Position = 0;
        var readBack = WavFormat.Instance.Read(stream);
        readBack.SampleRate.Should().Equal(wav.SampleRate);
        readBack.SampleData.Should().HaveCount(wav.SampleData.Length);
    }

    [Test]
    public void Convert_PauseBlockWithZeroDurationProducesNoSamples()
    {
        using var stream = new MemoryStream();
        stream.Write("PZXT"u8);
        stream.Write([0x02, 0x00, 0x00, 0x00]);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);

        stream.Write("PAUS"u8);
        stream.Write([0x04, 0x00, 0x00, 0x00]);
        stream.Write([0x00, 0x00, 0x00, 0x00]);

        stream.Position = 0;
        var pzx = PzxFormat.Instance.Read(stream);

        var wav = new PzxToWavConverter().Convert(pzx);

        wav.SampleData.Should().BeEmpty();
    }
}