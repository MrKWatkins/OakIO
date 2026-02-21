using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Wasm.Tests;

public sealed class ConvertTests : WasmTestFixture
{
    [Test]
    public void Convert_TapToWav_ReturnsNonEmptyData()
    {
        var result = Convert("test.tap", CreateTapData(), "output.wav");
        result.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_TapToWav_ProducesValidWav()
    {
        var result = Convert("test.tap", CreateTapData(), "output.wav");
        var wav = (WavFile)WavFormat.Instance.Read(result);
        wav.SampleData.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_TzxToWav_ReturnsNonEmptyData()
    {
        var result = Convert("test.tzx", CreateTzxData(), "output.wav");
        result.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_TzxToWav_ProducesValidWav()
    {
        var result = Convert("test.tzx", CreateTzxData(), "output.wav");
        var wav = (WavFile)WavFormat.Instance.Read(result);
        wav.SampleRate.Should().Equal(44100u);
    }

    [Test]
    public void Convert_PzxToWav_ReturnsNonEmptyData()
    {
        var result = Convert("test.pzx", CreatePzxData(), "output.wav");
        result.Should().NotBeEmpty();
    }

    [Test]
    public void Convert_PzxToWav_ProducesValidWav()
    {
        var result = Convert("test.pzx", CreatePzxData(), "output.wav");
        var wav = (WavFile)WavFormat.Instance.Read(result);
        wav.SampleRate.Should().Equal(44100u);
    }

    [Test]
    public void Convert_UnsupportedInputExtension_Throws()
    {
        AssertThat.Invoking(() => Convert("test.blah", CreateTapData(), "output.wav"))
            .Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Convert_UnsupportedOutputExtension_Throws()
    {
        AssertThat.Invoking(() => Convert("test.tap", CreateTapData(), "output.blah"))
            .Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Convert_UnsupportedConversion_Throws()
    {
        AssertThat.Invoking(() => Convert("test.z80", CreateZ80Data(), "output.wav"))
            .Should().Throw<NotSupportedException>();
    }
}