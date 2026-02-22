using MrKWatkins.OakIO.Tape;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Tests.Tape;

public sealed class TapeToWavConverterTests
{
    [Test]
    public void Convert_ReturnsSamplesAtCorrectRate()
    {
        var tape = new TapeFile([new PauseBlock(100, initialSignal: true)]);
        var converter = new TapeToWavConverter(100m);

        var wav = converter.Convert(tape, 10);

        wav.SampleRate.Should().Equal(10u);
        // tStatesPerSample = round(100 / 10) = 10. Pause=100 T-states.
        // 10 samples for the pause data + 1 final sample when the block finishes = 11 samples.
        wav.SampleData.Should().HaveCount(11);
    }

    [Test]
    public void Convert_HighSignal()
    {
        var tape = new TapeFile([new PauseBlock(100, initialSignal: true)]);
        var converter = new TapeToWavConverter(100m);

        var wav = converter.Convert(tape, 10);

        // Signal is true so first 10 samples should be high (0xC0).
        for (var i = 0; i < 10; i++)
        {
            wav.SampleData[i].Should().Equal(0xC0);
        }
    }

    [Test]
    public void Convert_LowSignal()
    {
        var tape = new TapeFile([new PauseBlock(100, initialSignal: false)]);
        var converter = new TapeToWavConverter(100m);

        var wav = converter.Convert(tape, 10);

        wav.SampleRate.Should().Equal(10u);
        wav.SampleData.Should().HaveCount(11);

        // Signal is false so first 10 samples should be low (0x40).
        for (var i = 0; i < 10; i++)
        {
            wav.SampleData[i].Should().Equal(0x40);
        }
    }

    [Test]
    public void Convert_DefaultSampleRate()
    {
        // Use tStatesPerSecond matching the default sample rate so tStatesPerSample = 1.
        var tape = new TapeFile([new PauseBlock(100, initialSignal: true)]);
        var converter = new TapeToWavConverter(IWavFileConverter.DefaultSampleRateHz);

        var wav = converter.Convert(tape);

        wav.SampleRate.Should().Equal(IWavFileConverter.DefaultSampleRateHz);
    }

    [Test]
    public void Convert_IOFile()
    {
        var tape = new TapeFile([new PauseBlock(100, initialSignal: true)]);
        var converter = new TapeToWavConverter(100m);

        var wav = converter.Convert((IOFile)tape, 10);

        wav.SampleRate.Should().Equal(10u);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_IOFile_ThrowsIfWrongType()
    {
        var converter = new TapeToWavConverter(100m);
        var wrongFile = new WrongFile();

        AssertThat.Invoking(() => converter.Convert(wrongFile, 10))
            .Should().ThrowArgumentException("Value is not of type TapeFile.", "source");
    }

    private sealed class WrongFile() : IOFile(WrongFileFormat.Instance);

    private sealed class WrongFileFormat() : IOFileFormat<WrongFile>("Wrong", "wrg")
    {
        public static readonly WrongFileFormat Instance = new();

        public override IOFile Read(Stream stream) => new WrongFile();

        protected override void Write(WrongFile file, Stream stream) { }
    }
}