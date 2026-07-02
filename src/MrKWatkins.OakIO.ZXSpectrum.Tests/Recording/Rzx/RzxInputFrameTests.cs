using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

public sealed class RzxInputFrameTests
{
    [Test]
    public void Constructor()
    {
        var frame = new RzxInputFrame(3, [0xFE, 0xFD]);

        frame.FetchCount.Should().Equal((ushort)3);
        frame.InputReads.Should().SequenceEqual(0xFE, 0xFD);
        frame.RepeatsPreviousInputReads.Should().BeFalse();
        frame.DataLength.Should().Equal(6U);
    }

    [Test]
    public void Constructor_Repeated()
    {
        var frame = new RzxInputFrame(4, repeatsPreviousInputReads: true);

        frame.InputReads.Should().BeEmpty();
        frame.RepeatsPreviousInputReads.Should().BeTrue();
        frame.DataLength.Should().Equal(4U);
    }

    [Test]
    public void Constructor_RepeatedWithData_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxInputFrame(1, [1], repeatsPreviousInputReads: true)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_TooManyInputReads_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxInputFrame(1, new byte[RzxInputFrame.RepeatedInputReads])).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Read()
    {
        var data = new byte[] { 3, 0, 2, 0, 0xFE, 0xFD };
        var offset = 0;

        var frame = RzxInputFrame.Read(data, ref offset);

        frame.FetchCount.Should().Equal((ushort)3);
        frame.InputReads.Should().SequenceEqual(0xFE, 0xFD);
        frame.RepeatsPreviousInputReads.Should().BeFalse();
        offset.Should().Equal(6);
    }

    [Test]
    public void Read_Repeated()
    {
        var data = new byte[] { 4, 0, 0xFF, 0xFF };
        var offset = 0;

        var frame = RzxInputFrame.Read(data, ref offset);

        frame.RepeatsPreviousInputReads.Should().BeTrue();
        offset.Should().Equal(4);
    }

    [Test]
    public void Read_Sequential()
    {
        // Two frames back to back: (fetch 3, reads [9]) then a repeated (fetch 4).
        var data = new byte[] { 3, 0, 1, 0, 9, 4, 0, 0xFF, 0xFF };
        var offset = 0;

        var first = RzxInputFrame.Read(data, ref offset);
        first.InputReads.Should().SequenceEqual(9);
        offset.Should().Equal(5);

        var second = RzxInputFrame.Read(data, ref offset);
        second.FetchCount.Should().Equal((ushort)4);
        second.RepeatsPreviousInputReads.Should().BeTrue();
        offset.Should().Equal(9);
    }

    [Test]
    public void Read_TruncatedHeader_Throws()
    {
        var data = new byte[] { 1, 0 };
        var offset = 0;

        AssertThat.Invoking(() => _ = RzxInputFrame.Read(data, ref offset)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_TruncatedInputReads_Throws()
    {
        var data = new byte[] { 1, 0, 5, 0, 1, 2 };
        var offset = 0;

        AssertThat.Invoking(() => _ = RzxInputFrame.Read(data, ref offset)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Write()
    {
        var frame = new RzxInputFrame(3, [0xFE, 0xFD]);
        var target = new byte[frame.DataLength];
        var offset = 0;

        frame.Write(target, ref offset);

        target.Should().SequenceEqual(3, 0, 2, 0, 0xFE, 0xFD);
        offset.Should().Equal(6);
    }

    [Test]
    public void Write_Repeated()
    {
        var frame = new RzxInputFrame(4, repeatsPreviousInputReads: true);
        var target = new byte[frame.DataLength];
        var offset = 0;

        frame.Write(target, ref offset);

        target.Should().SequenceEqual(4, 0, 0xFF, 0xFF);
        offset.Should().Equal(4);
    }
}