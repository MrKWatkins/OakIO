using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Tests.Wav;

public sealed class WavHeaderTests
{
    [Test]
    public void Constructor_Building()
    {
        var header = new WavHeader(44100, 5);

        header.SampleRate.Should().Equal(44100u);
        header.DataSize.Should().Equal(5);
        header.Length.Should().Equal(WavHeader.Size);

        var bytes = new byte[WavHeader.Size];
        header.CopyTo(bytes);
        bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8).Should().BeTrue();
        bytes.GetInt32(4).Should().Equal(5 + WavHeader.Size - 8);    // Chunk size.
        bytes.AsSpan(8, 4).SequenceEqual("WAVE"u8).Should().BeTrue();
        bytes.AsSpan(36, 4).SequenceEqual("data"u8).Should().BeTrue();
    }

    [Test]
    public void Constructor_MissingRiffHeader()
    {
        var bytes = ValidHeaderBytes();
        bytes[0] = (byte)'X';

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: missing RIFF header.");
    }

    [Test]
    public void Constructor_MissingWaveFormat()
    {
        var bytes = ValidHeaderBytes();
        bytes[8] = (byte)'X';

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: missing WAVE format.");
    }

    [Test]
    public void Constructor_MissingFmtSubchunk()
    {
        var bytes = ValidHeaderBytes();
        bytes[12] = (byte)'X';

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: missing fmt subchunk.");
    }

    [Test]
    public void Constructor_InvalidFmtSubchunkSize()
    {
        var bytes = ValidHeaderBytes();
        bytes.SetInt32(16, 18);

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: expected fmt subchunk size of 16 but got 18.");
    }

    [Test]
    public void Constructor_InvalidAudioFormat()
    {
        var bytes = ValidHeaderBytes();
        bytes.SetUInt16(20, 3);

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: expected PCM audio format (1) but got 3.");
    }

    [Test]
    public void Constructor_InvalidNumChannels()
    {
        var bytes = ValidHeaderBytes();
        bytes.SetUInt16(22, 2);

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: expected 1 channel but got 2.");
    }

    [Test]
    public void Constructor_InvalidBitsPerSample()
    {
        var bytes = ValidHeaderBytes();
        bytes.SetUInt16(34, 16);

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: expected 8 bits per sample but got 16.");
    }

    [Test]
    public void Constructor_MissingDataSubchunk()
    {
        var bytes = ValidHeaderBytes();
        bytes[36] = (byte)'X';

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: missing data subchunk.");
    }

    [Test]
    public void Constructor_NegativeDataSize()
    {
        var bytes = ValidHeaderBytes();
        bytes.SetInt32(40, -1);

        AssertThat.Invoking(() => new WavHeader(bytes)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: negative data subchunk size of -1.");
    }

    [Pure]
    private static byte[] ValidHeaderBytes(uint sampleRate = 44100, int dataLength = 0)
    {
        var bytes = new byte[WavHeader.Size];
        new WavHeader(sampleRate, dataLength).CopyTo(bytes);
        return bytes;
    }
}