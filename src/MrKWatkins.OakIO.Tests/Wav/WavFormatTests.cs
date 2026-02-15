using MrKWatkins.OakIO.Wav;

namespace MrKWatkins.OakIO.Tests.Wav;

public sealed class WavFormatTests
{
    [Test]
    public void Instance()
    {
        WavFormat.Instance.Name.Should().Equal("WAV");
        WavFormat.Instance.FileExtension.Should().Equal("wav");
    }

    [Test]
    public void RoundTrip()
    {
        byte[] sampleData = [0x80, 0xC0, 0x40, 0x60, 0xA0];
        var original = new WavFile(22050, sampleData);

        var bytes = WavFormat.Instance.Write(original);

        using var stream = new MemoryStream(bytes);
        var result = WavFormat.Instance.Read(stream);

        result.SampleRate.Should().Equal(22050u);
        result.SampleData.Should().SequenceEqual(sampleData);
    }

    [Test]
    public void Write_NonSeekableStream()
    {
        byte[] sampleData = [0x80, 0xC0, 0x40];
        var wavFile = new WavFile(44100, sampleData);

        using var memoryStream = new MemoryStream();
        using var nonSeekableStream = new NonSeekableStream(memoryStream);

        WavFormat.Instance.Write(wavFile, nonSeekableStream);

        memoryStream.Position = 0;
        var result = WavFormat.Instance.Read(memoryStream);
        result.SampleRate.Should().Equal(44100u);
        result.SampleData.Should().SequenceEqual(sampleData);
    }

    [Test]
    public void Read_MissingRiffHeader()
    {
        var bytes = new byte[] { (byte)'X', (byte)'I', (byte)'F', (byte)'F' };
        using var stream = new MemoryStream(bytes);

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: missing RIFF header.");
    }

    [Test]
    public void Read_MissingWaveFormat()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(0); // chunk size
        writer.Write("XXXX"u8); // not WAVE
        stream.Position = 0;

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: missing WAVE format.");
    }

    [Test]
    public void Read_MissingFmtSubchunk()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(0);
        writer.Write("WAVE"u8);
        writer.Write("xxxx"u8); // not "fmt "
        stream.Position = 0;

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: missing fmt subchunk.");
    }

    [Test]
    public void Read_InvalidFmtSubchunkSize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(0);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(18); // not 16
        stream.Position = 0;

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: expected fmt subchunk size of 16 but got 18.");
    }

    [Test]
    public void Read_InvalidAudioFormat()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(0);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((ushort)3); // not PCM (1)
        stream.Position = 0;

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: expected PCM audio format (1) but got 3.");
    }

    [Test]
    public void Read_InvalidNumChannels()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(0);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((ushort)1); // PCM
        writer.Write((ushort)2); // stereo, not mono
        stream.Position = 0;

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: expected 1 channel but got 2.");
    }

    [Test]
    public void Read_InvalidBitsPerSample()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(0);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((ushort)1); // PCM
        writer.Write((ushort)1); // mono
        writer.Write(44100u);    // sample rate
        writer.Write(88200u);    // byte rate
        writer.Write((ushort)2); // block align
        writer.Write((ushort)16); // 16 bits, not 8
        stream.Position = 0;

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: expected 8 bits per sample but got 16.");
    }

    [Test]
    public void Read_MissingDataSubchunk()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(0);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((ushort)1); // PCM
        writer.Write((ushort)1); // mono
        writer.Write(44100u);    // sample rate
        writer.Write(44100u);    // byte rate
        writer.Write((ushort)1); // block align
        writer.Write((ushort)8); // 8 bits
        writer.Write("xxxx"u8); // not "data"
        stream.Position = 0;

        AssertThat.Invoking(() => WavFormat.Instance.Read(stream)).Should().Throw<InvalidDataException>()
            .That.Message.Should().Equal("Not a valid WAV file: missing data subchunk.");
    }

    private sealed class NonSeekableStream(MemoryStream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => inner.Position = value; }

        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
    }
}
