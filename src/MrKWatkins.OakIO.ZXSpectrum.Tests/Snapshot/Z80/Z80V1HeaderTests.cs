using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

public sealed class Z80V1HeaderTests
{
    [Test]
    public void Constructor()
    {
        var header = new Z80V1Header();
        header.AsReadOnlySpan().Length.Should().Equal(30);
    }

    [Test]
    public void BorderColour([Values] ZXColour colour)
    {
        var bytes = new byte[34];
        var header = new Z80V1Header(bytes);

        header.BorderColour.Should().Equal(ZXColour.Black);
        header.BorderColour = colour;

        var expected = new byte[34];
        expected[12] = (byte)((int)colour << 1);
        bytes.Should().SequenceEqual(expected);

        bytes[12] = 0b11111111;
        header.BorderColour = colour;

        expected[12] = (byte)(0b11110001 | ((int)colour << 1));
        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    public void DataIsCompressed()
    {
        var bytes = new byte[34];
        var header = new Z80V1Header(bytes);

        header.DataIsCompressed.Should().BeFalse();
        header.DataIsCompressed = true;

        var expected = new byte[34];
        expected[12] = 0b00100000;
        bytes.Should().SequenceEqual(expected);

        bytes[12] = 0b11111111;
        header.DataIsCompressed = false;

        expected[12] = 0b11011111;
        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    public void InterruptFlipFlop()
    {
        var bytes = new byte[34];
        var header = new Z80V1Header(bytes);

        header.IFF1.Should().BeFalse();
        header.IFF1 = true;

        var expected = new byte[34];
        expected[27] = 1;
        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void IFF2()
    {
        var bytes = new byte[34];
        var header = new Z80V1Header(bytes);

        header.IFF2.Should().BeFalse();
        header.IFF2 = true;

        var expected = new byte[34];
        expected[28] = 1;
        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    public void InterruptMode([Values(0, 1, 2)] byte interruptMode)
    {
        var bytes = new byte[34];
        var header = new Z80V1Header(bytes);

        header.InterruptMode.Should().Equal(0);
        header.InterruptMode = interruptMode;

        var expected = new byte[34];
        expected[29] = interruptMode;
        bytes.Should().SequenceEqual(expected);

        bytes[29] = 0b11111111;
        header.InterruptMode = interruptMode;

        expected[29] = (byte)(0b11111100 | interruptMode);
        bytes.Should().SequenceEqual(expected);
    }

    [TestCase(0b00000000, VideoSynchronisation.Normal)]
    [TestCase(0b00010000, VideoSynchronisation.High)]
    [TestCase(0b00100000, VideoSynchronisation.Normal)]
    [TestCase(0b00110000, VideoSynchronisation.Low)]
    public void VideoSynchronisation_Get(byte headerValue, VideoSynchronisation expected)
    {
        var bytes = new byte[34];
        var header = new Z80V1Header(bytes);

        header.VideoSynchronisation.Should().Equal(VideoSynchronisation.Normal);

        bytes[29] = headerValue;
        header.VideoSynchronisation.Should().Equal(expected);

        bytes[29] = (byte)(0b11001111 | headerValue);
        header.VideoSynchronisation.Should().Equal(expected);
    }

    [TestCase(VideoSynchronisation.Normal, 0b00000000)]
    [TestCase(VideoSynchronisation.High, 0b00010000)]
    [TestCase(VideoSynchronisation.Low, 0b00110000)]
    public void VideoSynchronisation_Set(VideoSynchronisation value, byte expectedHeaderValue)
    {
        var bytes = new byte[34];
        var header = new Z80V1Header(bytes)
        {
            VideoSynchronisation = value
        };

        header.VideoSynchronisation.Should().Equal(value);
        var expected = new byte[34];
        expected[29] = expectedHeaderValue;
        bytes.Should().SequenceEqual(expected);

        bytes[29] = 0b11111111;
        header.VideoSynchronisation = value;
        header.VideoSynchronisation.Should().Equal(value);

        expected[29] = (byte)(0b11001111 | expectedHeaderValue);
        bytes.Should().SequenceEqual(expected);
    }

    [Test]
    public void Joystick([Values] Joystick joystick)
    {
        var bytes = new byte[34];
        var header = new Z80V1Header(bytes);

        header.Joystick.Should().Equal(ZXSpectrum.Snapshot.Z80.Joystick.Cursor);

        header.Joystick = joystick;

        var expectedByte = (byte)((int)joystick << 6);

        var expected = new byte[34];
        expected[29] = expectedByte;
        bytes.Should().SequenceEqual(expected);

        bytes[29] = 0b11111111;
        header.Joystick = joystick;

        expected[29] = (byte)(0b00111111 | expectedByte);
        bytes.Should().SequenceEqual(expected);
    }
}