using System.IO.Compression;
using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

public sealed class Z80CompressionStreamTests
{
    [TestCaseSource(nameof(ValidTestCases))]
    public void Read_AllBytes(byte[] data, byte[] compressed, bool endMarker)
    {
        var actual = ReadAllBytes(compressed, endMarker);
        actual.Should().SequenceEqual(data);
    }

    [TestCaseSource(nameof(ReadInvalidTestCases))]
    public void Read_AllBytes_Invalid(byte[] compressed, bool endMarker, string expectedMessage) =>
        AssertThat.Invoking(() => ReadAllBytes(compressed, endMarker))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Equal(expectedMessage);

    [Pure]
    private static IReadOnlyList<byte> ReadAllBytes(byte[] compressed, bool endMarker)
    {
        using var memoryStream = new MemoryStream(compressed);
        using var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Decompress, endMarker);

        return z80Stream.ReadAllBytes();
    }

    [TestCaseSource(nameof(ValidTestCases))]
    public void Read_OneByteAtATime(byte[] data, byte[] compressed, bool endMarker)
    {
        // Read single bytes to test resuming in the middle of state.
        var actual = ReadOneByteAtATime(compressed, endMarker);
        actual.Should().SequenceEqual(data);
    }

    [TestCaseSource(nameof(ReadInvalidTestCases))]
    public void Read_OneByteAtATime_Invalid(byte[] compressed, bool endMarker, string expectedMessage) =>
        AssertThat.Invoking(() => ReadOneByteAtATime(compressed, endMarker))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Equal(expectedMessage);

    [Pure]
    private static IReadOnlyList<byte> ReadOneByteAtATime(byte[] compressed, bool endMarker)
    {
        using var memoryStream = new MemoryStream(compressed);
        using var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Decompress, endMarker);

        var actual = new List<byte>();
        int value;
        while ((value = z80Stream.ReadByte()) != -1)
        {
            actual.Add((byte)value);
        }

        return actual;
    }

    [TestCaseSource(nameof(ValidTestCases))]
    public void Read_TwoBytesAtATime(byte[] data, byte[] compressed, bool endMarker)
    {
        // Read in two byte chunks to test resuming in the middle of state.
        var actual = ReadASliceAtATime(compressed, endMarker, 2);
        actual.Should().SequenceEqual(data);
    }

    [TestCaseSource(nameof(ReadInvalidTestCases))]
    public void Read_TwoBytesAtATime_Invalid(byte[] compressed, bool endMarker, string expectedMessage) =>
        AssertThat.Invoking(() => ReadASliceAtATime(compressed, endMarker, 2))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Equal(expectedMessage);

    [TestCaseSource(nameof(ValidTestCases))]
    public void Read_ThreeBytesAtATime(byte[] data, byte[] compressed, bool endMarker)
    {
        // Read in two byte chunks to test resuming in the middle of state.
        var actual = ReadASliceAtATime(compressed, endMarker, 3);
        actual.Should().SequenceEqual(data);
    }

    [TestCaseSource(nameof(ReadInvalidTestCases))]
    public void Read_ThreeBytesAtATime_Invalid(byte[] compressed, bool endMarker, string expectedMessage) =>
        AssertThat.Invoking(() => ReadASliceAtATime(compressed, endMarker, 3))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Equal(expectedMessage);

    [Pure]
    private static IReadOnlyList<byte> ReadASliceAtATime(byte[] compressed, bool endMarker, int sliceSize)
    {
        // Read in slices to test resuming in the middle of state such as a repeater.
        using var memoryStream = new MemoryStream(compressed);
        using var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Decompress, endMarker);

        var actual = new List<byte>();
        Span<byte> buffer = stackalloc byte[sliceSize];
        int bytesRead;
        do
        {
            bytesRead = z80Stream.Read(buffer);
            foreach (var @byte in buffer[..bytesRead])
            {
                actual.Add(@byte);
            }

        } while (bytesRead == sliceSize);

        return actual;
    }

    [Test]
    public void Read_CompressStreamThrows()
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Compress);
        z80Stream.Invoking(s => s.ReadByte())
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Equal("This stream is for decompressing only.");
    }

    [Test]
    public void Read_DisposedStreamThrows()
    {
        using var memoryStream = new MemoryStream();
        var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Compress);
        z80Stream.Dispose();

        z80Stream.Invoking(s => s.ReadByte())
            .Should().Throw<ObjectDisposedException>()
            .Exception.Message.Should().Equal($"Cannot access a disposed object.{Environment.NewLine}Object name: 'Z80CompressionStream'.");
    }

    [TestCaseSource(nameof(ValidTestCases))]
    public void Write_AllBytes(byte[] data, byte[] compressed, bool endMarker)
    {
        var actual = WriteAllBytes(data, endMarker);
        actual.Should().SequenceEqual(compressed);
    }

    [Pure]
    private static IReadOnlyList<byte> WriteAllBytes(byte[] data, bool endMarker)
    {
        using var memoryStream = new MemoryStream();
        using (var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Compress, endMarker))
        {
            z80Stream.Write(data);
        }

        return memoryStream.ToArray();
    }

    [TestCaseSource(nameof(ValidTestCases))]
    public void Write_OneByteAtATime(byte[] data, byte[] compressed, bool endMarker)
    {
        var actual = WriteOneByteAtATime(data, endMarker);
        actual.Should().SequenceEqual(compressed);
    }

    [Pure]
    private static IReadOnlyList<byte> WriteOneByteAtATime(byte[] data, bool endMarker)
    {
        using var memoryStream = new MemoryStream();
        using (var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Compress, endMarker))
        {
            foreach (var @byte in data)
            {
                z80Stream.WriteByte(@byte);
            }
        }

        return memoryStream.ToArray();
    }

    [TestCaseSource(nameof(ValidTestCases))]
    public void Write_TwoBytesAtATime(byte[] data, byte[] compressed, bool endMarker)
    {
        var actual = WriteASliceAtATime(data, endMarker, 2);
        actual.Should().SequenceEqual(compressed);
    }

    [TestCaseSource(nameof(ValidTestCases))]
    public void Write_ThreeBytesAtATime(byte[] data, byte[] compressed, bool endMarker)
    {
        var actual = WriteASliceAtATime(data, endMarker, 3);
        actual.Should().SequenceEqual(compressed);
    }

    [Pure]
    private static IReadOnlyList<byte> WriteASliceAtATime(byte[] data, bool endMarker, int sliceSize)
    {
        using var memoryStream = new MemoryStream();
        using (var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Compress, endMarker))
        {
            foreach (var chunk in data.Chunk(sliceSize))
            {
                z80Stream.Write(chunk.ToArray());
            }
        }

        return memoryStream.ToArray();
    }

    [Test]
    public void Write_DecompressStreamThrows()
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Decompress);
        z80Stream.Invoking(s => s.WriteByte(0x00))
            .Should().Throw<InvalidOperationException>()
            .Exception.Message.Should().Equal("This stream is for compressing only.");
    }

    [Test]
    public void Write_DisposedStreamThrows()
    {
        using var memoryStream = new MemoryStream();
        var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Compress);
        z80Stream.Dispose();

        z80Stream.Invoking(s => s.WriteByte(0x00))
            .Should().Throw<ObjectDisposedException>()
            .Exception.Message.Should().Equal($"Cannot access a disposed object.{Environment.NewLine}Object name: 'Z80CompressionStream'.");
    }

    [TestCaseSource(nameof(ValidTestCases))]
    public void Dispose_CallingTwiceDoesNotWriteEndTwice(byte[] data, byte[] compressed, bool endMarker)
    {
        using var memoryStream = new MemoryStream();
        var z80Stream = new Z80CompressionStream(memoryStream, CompressionMode.Compress, endMarker);
        z80Stream.Write(data);
        z80Stream.Dispose();
        z80Stream.Dispose();

        var actual = memoryStream.ToArray();
        actual.Should().SequenceEqual(compressed);
    }

    [Test]
    public void Dispose_DisposesUnderlyingStreamAccordingToLeaveOpen([Values(true, false)] bool leaveOpen)
    {
        using var testStream = new TestStream();
        var z80Stream = new Z80CompressionStream(testStream, CompressionMode.Decompress, leaveOpen: leaveOpen);
        z80Stream.Dispose();
        testStream.IsDisposed.Should().Equal(!leaveOpen);
    }

    [TestCase(CompressionMode.Compress, false)]
    [TestCase(CompressionMode.Decompress, true)]
    public void CanRead(CompressionMode compressionMode, bool expected)
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, compressionMode);
        z80Stream.CanRead.Should().Equal(expected);
    }

    [TestCase(CompressionMode.Compress, false)]
    [TestCase(CompressionMode.Decompress, false)]
    public void CanSeek(CompressionMode compressionMode, bool expected)
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, compressionMode);
        z80Stream.CanSeek.Should().Equal(expected);
    }

    [TestCase(CompressionMode.Compress, true)]
    [TestCase(CompressionMode.Decompress, false)]
    public void CanWrite(CompressionMode compressionMode, bool expected)
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, compressionMode);
        z80Stream.CanWrite.Should().Equal(expected);
    }

    [TestCase(CompressionMode.Compress)]
    [TestCase(CompressionMode.Decompress)]
    public void Length_Throws(CompressionMode compressionMode)
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, compressionMode);
        z80Stream.Invoking(s => s.Length).Should().Throw<NotSupportedException>();
    }

    [TestCase(CompressionMode.Compress)]
    [TestCase(CompressionMode.Decompress)]
    public void Position_Throws(CompressionMode compressionMode)
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, compressionMode);
        z80Stream.Invoking(s => s.Position).Should().Throw<NotSupportedException>();
        z80Stream.Invoking(s => s.Position = 0).Should().Throw<NotSupportedException>();
    }

    [TestCase(CompressionMode.Compress)]
    [TestCase(CompressionMode.Decompress)]
    public void Seek_Throws(CompressionMode compressionMode)
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, compressionMode);
        z80Stream.Invoking(s => s.Seek(0, SeekOrigin.Begin)).Should().Throw<NotSupportedException>();
    }

    [TestCase(CompressionMode.Compress)]
    [TestCase(CompressionMode.Decompress)]
    public void SetLength_Throws(CompressionMode compressionMode)
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, compressionMode);
        z80Stream.Invoking(s => s.SetLength(10)).Should().Throw<NotSupportedException>();
    }

    [TestCase(CompressionMode.Compress)]
    [TestCase(CompressionMode.Decompress)]
    public void Flush_DoesNothing(CompressionMode compressionMode)
    {
        using var memoryStream = new MemoryStream();
        using var z80Stream = new Z80CompressionStream(memoryStream, compressionMode);
        z80Stream.Flush();
        memoryStream.ToArray().Should().BeEmpty();
    }

    [Pure]
    public static IEnumerable<TestCaseData> ValidTestCases()
    {
        static TestCaseData CreateTestCase(byte[] data, byte[] compressed, bool endMarker) =>
            new TestCaseData(data, compressed, endMarker)
                .SetName($"Data = [{FormatArray(data)}], Compressed = [{FormatArray(compressed)}], End Marker = {endMarker}");

        var endMarker = new byte[] { 0x00, 0xED, 0xED, 0x00 };
        foreach (var (data, compressed) in RawValidTestCases())
        {
            yield return CreateTestCase(data, compressed, false);

            yield return CreateTestCase(data, compressed.Concat(endMarker).ToArray(), true);
        }
    }

    [Pure]
    [SuppressMessage("ReSharper", "UseUtf8StringLiteral")]
    private static IEnumerable<(byte[] Data, byte[] Compressed)> RawValidTestCases()
    {
        yield return (
            [],
            []);
        yield return (
            [0x00],
            [0x00]);
        yield return (
            [0x00, 0x00],
            [0x00, 0x00]);
        yield return (
            [0x00, 0x00, 0x00, 0x00, 0x00],
            [0xED, 0xED, 0x05, 0x00]);
        yield return (
            [0x00, 0x01],
            [0x00, 0x01]);
        yield return (
            [0x01, 0x02, 0x03],
            [0x01, 0x02, 0x03]);
        yield return (
            [0x01, 0x01, 0x01, 0x01],
            [0x01, 0x01, 0x01, 0x01]);
        yield return (
            [0xED],
            [0xED]);
        yield return (
            [0x00, 0xED, 0x00],
            [0x00, 0xED, 0x00]);
        yield return (
            [0xED, 0xED],
            [0xED, 0xED, 0x02, 0xED]);
        yield return (
            [0x01, 0xED, 0x04, 0x05],
            [0x01, 0xED, 0x04, 0x05]);
        yield return (
            [0x03, 0x03, 0x03, 0x03, 0x03, 0x04],
            [0xED, 0xED, 0x05, 0x03, 0x04]);
        yield return (
            [0x01, 0x03, 0x03, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04],
            [0x01, 0xED, 0xED, 0x05, 0x03, 0xED, 0xED, 0x06, 0x04]);
        yield return (
            [0xED, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00],
            [0xED, 0x00, 0xED, 0xED, 0x05, 0x00]);

        yield return (
            Enumerable.Repeat((byte)0, 260).ToArray(),
            [0xED, 0xED, 0xFF, 0x00, 0xED, 0xED, 0x05, 0x00]);
    }

    [Pure]
    public static IEnumerable<TestCaseData> ReadInvalidTestCases()
    {
        static TestCaseData CreateTestCase(byte[] data, bool endMarker, string expectedMessage) =>
            new TestCaseData(data, endMarker, expectedMessage)
                .SetName($"Data = [{FormatArray(data)}], End Marker = {endMarker}, Expected Message = \"{expectedMessage}\"");

        yield return CreateTestCase([0xED], true, "Found truncated data while decompressing; data finished in the middle of a encoding section.");
        yield return CreateTestCase([0xED, 0xED], false, "Found truncated data while decompressing; data finished in the middle of a encoding section.");
        yield return CreateTestCase([0xED, 0xED, 0x02], false, "Found truncated data while decompressing; data finished in the middle of a encoding section.");
        yield return CreateTestCase([0xED, 0xED, 0x03, 0x04], false, "Found invalid data while decompressing; repeated sections not of 0xED should have length 5 or greater, found 3.");
        yield return CreateTestCase([0xED, 0xED, 0x01, 0xED], false, "Found invalid data while decompressing; repeated sections of 0xED should have length 2 greater, found 1.");
        yield return CreateTestCase([0x00, 0xED, 0xED, 0x00], false, "Found truncated data while decompressing; data finished in the middle of a encoding section.");
        yield return CreateTestCase([0x01, 0x02], true, "Found truncated data while decompressing; missing end marker.");
    }

    [Pure]
    private static string FormatArray(byte[] bytes)
    {
        if (bytes.Length > 12)
        {
            return string.Join(" ", bytes.Take(6).Select(b => $"0x{b:X2}")) + " ... " + string.Join(" ", bytes.TakeLast(6).Select(b => $"0x{b:X2}"));
        }
        return string.Join(" ", bytes.Select(b => $"0x{b:X2}"));
    }

    private sealed class TestStream : Stream
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override bool CanRead => throw new NotSupportedException();
        public override bool CanSeek => throw new NotSupportedException();
        public override bool CanWrite => throw new NotSupportedException();
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}