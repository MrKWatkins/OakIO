using System.IO.Compression;
using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

public sealed class RzxFormatTests
{
    [Test]
    public void Instance()
    {
        RzxFormat.Instance.Name.Should().Equal("RZX Input Recording");
        RzxFormat.Instance.FileExtension.Should().Equal("rzx");
        RzxFormat.Instance.CanRead.Should().BeTrue();
        RzxFormat.Instance.CanWrite.Should().BeTrue();
    }

    [Test]
    public void ReadWrite_RoundTrip()
    {
        var file = CreateRzxFile();
        var bytes = file.ToByteArray();

        var loaded = RzxFormat.Instance.Read(bytes);

        loaded.Header.Signature.Should().Equal("RZX!");
        loaded.Header.MajorVersion.Should().Equal((byte)0);
        loaded.Header.MinorVersion.Should().Equal((byte)13);

        loaded.Creator.Creator.Should().Equal("OakEmu");
        loaded.Creator.MajorVersion.Should().Equal((ushort)1);
        loaded.Creator.MinorVersion.Should().Equal((ushort)2);
        loaded.Creator.CustomData.ToArray().Should().SequenceEqual(1, 2);

        loaded.Snapshot.Should().NotBeNull();
        loaded.Snapshot!.Extension.Should().Equal("Z80");
        loaded.Snapshot.SnapshotData.Should().SequenceEqual(0xAA, 0xBB);

        var recording = loaded.InputRecordings.Single();
        recording.StartTStates.Should().Equal(123U);
        recording.Frames.Should().HaveCount(2);
        recording.Frames[0].FetchCount.Should().Equal((ushort)3);
        recording.Frames[0].InputReads.Should().SequenceEqual(0xFE, 0xFD);
        recording.Frames[0].RepeatsPreviousInputReads.Should().BeFalse();
        recording.Frames[1].FetchCount.Should().Equal((ushort)4);
        recording.Frames[1].InputReads.Should().BeEmpty();
        recording.Frames[1].RepeatsPreviousInputReads.Should().BeTrue();
    }

    [Test]
    public void Read_Stream()
    {
        using var stream = new MemoryStream(CreateRzxFile().ToByteArray());

        var loaded = RzxFormat.Instance.Read(stream);

        loaded.Creator.Creator.Should().Equal("OakEmu");
    }

    [Test]
    public async Task ReadAsync_Stream()
    {
        using var stream = new MemoryStream(CreateRzxFile().ToByteArray());

        var loaded = await RzxFormat.Instance.ReadAsync(stream);

        loaded.Creator.Creator.Should().Equal("OakEmu");
    }

    [Test]
    public void Read_InvalidSignature_Throws()
    {
        var bytes = new byte[10];
        "NotR"u8.CopyTo(bytes);

        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_UnsupportedVersion_Throws()
    {
        var bytes = CreateRzxFile().ToByteArray();
        bytes[4] = 1;

        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_MissingCreator_Throws()
    {
        var file = new RzxFile([new RzxInputRecordingBlock([new RzxInputFrame(1)])]);
        var bytes = file.ToByteArray();

        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_MissingInputRecording_Throws()
    {
        var file = new RzxFile([new RzxCreatorBlock("OakEmu", 1, 0)]);
        var bytes = file.ToByteArray();

        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [TestCase(RzxBlockType.SecurityInformation)]
    [TestCase(RzxBlockType.SecuritySignature)]
    public void Read_SecurityBlock_Throws(RzxBlockType type)
    {
        using var stream = new MemoryStream();
        new RzxHeader().Write(stream);
        WriteRawBlock(stream, type, new byte[8]);

        var bytes = stream.ToArray();
        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read(bytes)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Read_EmptyStream_Throws()
    {
        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read([])).Should().Throw<EndOfStreamException>();
    }

    [Test]
    public void Read_TruncatedHeader_Throws()
    {
        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read(new byte[5])).Should().Throw<EndOfStreamException>();
    }

    [Test]
    public void Read_TruncatedBlockBody_Throws()
    {
        using var stream = new MemoryStream();
        new RzxHeader().Write(stream);
        // A creator block header claiming a 95 byte body, but no body follows.
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true))
        {
            writer.Write((byte)RzxBlockType.Creator);
            writer.Write(100U);
        }

        var bytes = stream.ToArray();
        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read(bytes)).Should().Throw<EndOfStreamException>();
    }

    [Test]
    public void WriteAsync_WrongFileType_Throws()
    {
        var notAnRzxFile = TapFile.CreateCode("code", 0x8000, [1, 2, 3]);

        AssertThat.Invoking(() => RzxFormat.Instance.WriteAsync(notAnRzxFile, new StubBinaryWriter())).Should().Throw<ArgumentException>();
    }

    [Test]
    public void ReadWrite_MultipleInputRecordings_RoundTrip()
    {
        var file = new RzxFile(
        [
            new RzxCreatorBlock("OakEmu", 1, 0),
            new RzxSnapshotBlock("Z80", [0x01]),
            new RzxInputRecordingBlock([new RzxInputFrame(1, [0xAA])], 10),
            new RzxSnapshotBlock("Z80", [0x02]),
            new RzxInputRecordingBlock([new RzxInputFrame(2, [0xBB])], 20)
        ]);

        var loaded = RzxFormat.Instance.Read(file.ToByteArray());

        loaded.InputRecordings.Should().HaveCount(2);
        loaded.InputRecordings[0].StartTStates.Should().Equal(10U);
        loaded.InputRecordings[1].StartTStates.Should().Equal(20U);
        loaded.Blocks.Should().HaveCount(5);
    }

    [Test]
    public void ReadWrite_HeaderFlags_RoundTrip()
    {
        var file = new RzxFile(CreateRzxFile().Blocks, new RzxHeader(0xDEADBEEF));

        var loaded = RzxFormat.Instance.Read(file.ToByteArray());

        loaded.Header.Flags.Should().Equal(0xDEADBEEFU);
    }

    [Test]
    public void Load_ByExtension()
    {
        using var stream = new MemoryStream(CreateRzxFile().ToByteArray());

        var loaded = ZXSpectrumFileFormat.Load("recording.rzx", stream);

        loaded.Should().BeOfType<RzxFile>();
    }

    [Test]
    public void Read_UnknownBlockType_Throws()
    {
        using var stream = new MemoryStream();
        new RzxHeader().Write(stream);
        WriteRawBlock(stream, (RzxBlockType)0x99, []);

        var bytes = stream.ToArray();
        AssertThat.Invoking(() => _ = RzxFormat.Instance.Read(bytes)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Read_CompressedBlocks_V012()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true))
        {
            writer.Write("RZX!"u8);
            writer.Write((byte)0);
            writer.Write((byte)12);
            writer.Write(0U);
        }

        WriteRawBlock(stream, new RzxCreatorBlock("OakEmu", 1, 2));
        WriteCompressedSnapshotBlock(stream, [0xAA, 0xBB, 0xCC]);
        WriteCompressedInputRecordingBlock(stream);

        var loaded = RzxFormat.Instance.Read(stream.ToArray());

        loaded.Header.MinorVersion.Should().Equal((byte)12);
        loaded.Snapshot.Should().NotBeNull();
        loaded.Snapshot!.Flags.Should().Equal(RzxSnapshotFlags.Compressed);
        loaded.Snapshot.Extension.Should().Equal("z80");
        loaded.Snapshot.UncompressedLength.Should().Equal(3U);
        loaded.Snapshot.SnapshotData.Should().SequenceEqual(0xAA, 0xBB, 0xCC);

        var recording = loaded.InputRecordings.Single();
        recording.Flags.Should().Equal(RzxInputRecordingFlags.Compressed);
        recording.StartTStates.Should().Equal(123U);
        recording.Frames.Should().HaveCount(2);
        recording.Frames[0].FetchCount.Should().Equal((ushort)3);
        recording.Frames[0].InputReads.Should().SequenceEqual(0xFE, 0xFD);
        recording.Frames[1].FetchCount.Should().Equal((ushort)4);
        recording.Frames[1].RepeatsPreviousInputReads.Should().BeTrue();
    }

    [Pure]
    internal static RzxFile CreateRzxFile() =>
        new(
        [
            new RzxCreatorBlock("OakEmu", 1, 2, [1, 2]),
            new RzxSnapshotBlock("Z80", [0xAA, 0xBB]),
            new RzxInputRecordingBlock(
            [
                new RzxInputFrame(3, [0xFE, 0xFD]),
                new RzxInputFrame(4, repeatsPreviousInputReads: true)
            ], 123)
        ]);

    private static void WriteRawBlock(Stream stream, RzxBlock block)
    {
        block.Header.Write(stream);
        block.Write(stream);
    }

    private static void WriteRawBlock(Stream stream, RzxBlockType type, byte[] body)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true);
        writer.Write((byte)type);
        writer.Write(checked((uint)(5 + body.Length)));
        writer.Write(body);
    }

    private static void WriteCompressedSnapshotBlock(Stream stream, byte[] snapshot)
    {
        using var payload = new MemoryStream();
        using (var writer = new BinaryWriter(payload, System.Text.Encoding.ASCII, true))
        {
            writer.Write((uint)RzxSnapshotFlags.Compressed);
            writer.Write("z80\0"u8);
            writer.Write(checked((uint)snapshot.Length));
            writer.Write(Compress(snapshot));
        }

        WriteRawBlock(stream, RzxBlockType.Snapshot, payload.ToArray());
    }

    private static void WriteCompressedInputRecordingBlock(Stream stream)
    {
        using var frames = new MemoryStream();
        using (var writer = new BinaryWriter(frames, System.Text.Encoding.ASCII, true))
        {
            writer.Write((ushort)3);
            writer.Write((ushort)2);
            writer.Write(new byte[] { 0xFE, 0xFD });
            writer.Write((ushort)4);
            writer.Write(RzxInputFrame.RepeatedInputReads);
        }

        using var payload = new MemoryStream();
        using (var writer = new BinaryWriter(payload, System.Text.Encoding.ASCII, true))
        {
            writer.Write(2U);
            writer.Write((byte)0);
            writer.Write(123U);
            writer.Write((uint)RzxInputRecordingFlags.Compressed);
            writer.Write(Compress(frames.ToArray()));
        }

        WriteRawBlock(stream, RzxBlockType.InputRecording, payload.ToArray());
    }

    [Pure]
    private static byte[] Compress(byte[] data)
    {
        using var stream = new MemoryStream();
        using (var zLib = new ZLibStream(stream, CompressionMode.Compress, true))
        {
            zLib.Write(data);
        }

        return stream.ToArray();
    }

    private sealed class StubBinaryWriter : IBinaryWriter
    {
        public ValueTask WriteAsync(ReadOnlyMemory<byte> value) => ValueTask.CompletedTask;
    }
}