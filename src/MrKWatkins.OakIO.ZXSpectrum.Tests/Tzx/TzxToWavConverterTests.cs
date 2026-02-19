using MrKWatkins.OakIO.Wav;
using MrKWatkins.OakIO.ZXSpectrum.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tzx;

public sealed class TzxToWavConverterTests
{
    [Test]
    public void Convert_ReturnsSamplesAtDefaultRate()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = TapToTzxConverter.Instance.Convert(tap);

        var wav = new TzxToWavConverter().Convert(tzx);

        wav.SampleRate.Should().Equal(44100u);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_CustomSampleRate()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = TapToTzxConverter.Instance.Convert(tap);

        var wav = new TzxToWavConverter(sampleRateHz: 22050).Convert(tzx);

        wav.SampleRate.Should().Equal(22050u);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_SamplesContainBothHighAndLow()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = TapToTzxConverter.Instance.Convert(tap);

        var wav = new TzxToWavConverter().Convert(tzx);

        wav.SampleData.Any(s => s == 0xC0).Should().BeTrue();
        wav.SampleData.Any(s => s == 0x40).Should().BeTrue();
    }

    [Test]
    public void Convert_WavCanBeWrittenAndRead()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var tzx = TapToTzxConverter.Instance.Convert(tap);

        var wav = new TzxToWavConverter().Convert(tzx);

        using var stream = new MemoryStream();
        WavFormat.Instance.Write(wav, stream);
        stream.Position = 0;
        var readBack = WavFormat.Instance.Read(stream);
        readBack.SampleRate.Should().Equal(wav.SampleRate);
        readBack.SampleData.Should().HaveCount(wav.SampleData.Length);
    }

    [Test]
    public void Convert_PureToneBlock()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        stream.WriteByte(0x12);
        stream.Write([0x78, 0x08]);
        stream.Write([0x0A, 0x00]);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var wav = new TzxToWavConverter().Convert(tzx);

        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_PauseBlock()
    {
        using var stream = new MemoryStream();
        stream.Write("ZXTape!\x1A"u8);
        stream.WriteByte(0x01);
        stream.WriteByte(0x14);
        stream.WriteByte(0x20);
        stream.Write([0xE8, 0x03]);
        stream.Position = 0;
        var tzx = TzxFormat.Instance.Read(stream);

        var wav = new TzxToWavConverter().Convert(tzx);

        wav.SampleData.Should().NotBeEmpty();
    }
}
