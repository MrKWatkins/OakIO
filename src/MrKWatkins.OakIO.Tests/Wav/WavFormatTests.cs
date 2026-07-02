using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Tests.Wav;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class WavFormatTests
{
    [Test]
    public void Instance()
    {
        WavFormat.Instance.Name.Should().Equal("WAV Audio");
        WavFormat.Instance.FileExtension.Should().Equal("wav");
    }

    [Test]
    public void RoundTrip()
    {
        byte[] sampleData = [0x80, 0xC0, 0x40, 0x60, 0xA0];
        var original = new WavFile(22050, sampleData);

        var bytes = original.ToByteArray();

        using var stream = new MemoryStream(bytes);
        var result = WavFormat.Instance.Read(stream);

        result.SampleRate.Should().Equal(22050u);
        result.SampleData.Should().SequenceEqual(sampleData);
    }

    [Test]
    public async Task Write()
    {
        byte[] sampleData = [0x80, 0xC0, 0x40];
        var wavFile = new WavFile(44100, sampleData);

        using var memoryStream = new MemoryStream();
        using (var writer = new SyncStreamBinaryWriter(memoryStream))
        {
            await WavFormat.Instance.WriteAsync(wavFile, writer);
        }

        memoryStream.Position = 0;
        var result = (WavFile)await WavFormat.Instance.ReadAsync(memoryStream);
        result.SampleRate.Should().Equal(44100u);
        result.SampleData.Should().SequenceEqual(sampleData);
    }

    [Test]
    public void Read_InvalidHeader()
    {
        var bytes = "XIFF"u8.ToArray();
        using var stream = new MemoryStream(bytes);

        // A header shorter than the fixed 44 bytes cannot be read at all.
        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<EndOfStreamException>();
    }

    [Test]
    public void Read_TruncatedHeader()
    {
        var bytes = new byte[WavHeader.Size - 1];
        using var stream = new MemoryStream(bytes);

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<EndOfStreamException>();
    }

    [Test]
    public void Read_TruncatedSampleData()
    {
        // A valid header claiming 10 bytes of sample data, but only 3 present.
        var bytes = new byte[WavHeader.Size + 3];
        new WavFile(44100, new byte[10]).ToByteArray().AsSpan(0, WavHeader.Size).CopyTo(bytes);
        using var stream = new MemoryStream(bytes);

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<EndOfStreamException>();
    }
}