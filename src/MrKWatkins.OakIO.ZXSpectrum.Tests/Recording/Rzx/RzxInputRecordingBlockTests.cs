using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

public sealed class RzxInputRecordingBlockTests
{
    [Test]
    public void Constructor()
    {
        var block = new RzxInputRecordingBlock(
        [
            new RzxInputFrame(3, [0xFE, 0xFD]),
            new RzxInputFrame(4, repeatsPreviousInputReads: true)
        ], 123);

        block.Type.Should().Equal(RzxBlockType.InputRecording);
        block.StartTStates.Should().Equal(123U);
        block.Flags.Should().Equal(RzxInputRecordingFlags.None);
        block.Frames.Should().HaveCount(2);
        block.Frames[0].FetchCount.Should().Equal((ushort)3);
        block.Frames[0].InputReads.Should().SequenceEqual(0xFE, 0xFD);
        block.Frames[1].RepeatsPreviousInputReads.Should().BeTrue();
    }

    [Test]
    public void Constructor_Bytes()
    {
        var block = new RzxInputRecordingBlock([new RzxInputFrame(5, [7])], 0x11223344);

        // Block header: ID 0x80, block length = 5 + 18 = 23.
        block.Header.Data.Should().SequenceEqual(0x80, 0x17, 0x00, 0x00, 0x00);

        block.Data.Should().SequenceEqual(
            0x01, 0x00, 0x00, 0x00,       // Frame count = 1.
            0x00,                          // Reserved.
            0x44, 0x33, 0x22, 0x11,       // Start T-states = 0x11223344.
            0x00, 0x00, 0x00, 0x00,       // Flags = None.
            0x05, 0x00, 0x01, 0x00, 0x07); // Frame: fetch 5, 1 input read of 0x07.
    }

    [Test]
    public void Constructor_NoFrames()
    {
        var block = new RzxInputRecordingBlock([]);

        block.Frames.Should().BeEmpty();
        // Frame count is zero and no frame data follows the 13 byte header.
        block.Data.Should().SequenceEqual(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    [Test]
    public void Constructor_MultipleFrames()
    {
        var block = new RzxInputRecordingBlock(
        [
            new RzxInputFrame(1, [0x10, 0x11]),
            new RzxInputFrame(2, repeatsPreviousInputReads: true),
            new RzxInputFrame(3, [0x20])
        ]);

        block.Frames.Should().HaveCount(3);
        block.Frames[0].InputReads.Should().SequenceEqual(0x10, 0x11);
        block.Frames[1].RepeatsPreviousInputReads.Should().BeTrue();
        block.Frames[2].FetchCount.Should().Equal((ushort)3);
        block.Frames[2].InputReads.Should().SequenceEqual(0x20);
    }

    [Test]
    public void Constructor_NullFrames_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxInputRecordingBlock(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_Protected_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxInputRecordingBlock([new RzxInputFrame(1)], flags: RzxInputRecordingFlags.Protected))
            .Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_TooShort_Throws()
    {
        var header = new RzxBlockHeader(RzxBlockType.InputRecording, 8);

        AssertThat.Invoking(() => _ = new RzxInputRecordingBlock(header, new byte[8])).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_ReservedByteNonZero_Throws()
    {
        var body = CreateBody(reserved: 1, flags: RzxInputRecordingFlags.None);
        var header = new RzxBlockHeader(RzxBlockType.InputRecording, (uint)body.Length);

        AssertThat.Invoking(() => _ = new RzxInputRecordingBlock(header, body)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_Protected_Throws()
    {
        var body = CreateBody(reserved: 0, flags: RzxInputRecordingFlags.Protected);
        var header = new RzxBlockHeader(RzxBlockType.InputRecording, (uint)body.Length);

        AssertThat.Invoking(() => _ = new RzxInputRecordingBlock(header, body)).Should().Throw<NotSupportedException>();
    }

    private static byte[] CreateBody(byte reserved, RzxInputRecordingFlags flags)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true))
        {
            writer.Write(0U);
            writer.Write(reserved);
            writer.Write(0U);
            writer.Write((uint)flags);
        }

        return stream.ToArray();
    }
}