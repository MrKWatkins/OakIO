using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Tests.Binary;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class SyncStreamBinaryReaderTests
{
    [Test]
    public async Task ReadAsync()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        using var binaryReader = new SyncStreamBinaryReader(stream);
        IBinaryReader reader = binaryReader;

        var read = reader.ReadAsync(2);
        read.IsCompleted.Should().BeTrue();    // The synchronous reader completes synchronously.
        (await read).Should().SequenceEqual(1, 2);

        (await reader.ReadAsync(3)).Should().SequenceEqual(3, 4, 5);
    }

    [Test]
    public async Task ReadAsync_Zero()
    {
        using var stream = new MemoryStream([1]);
        using var binaryReader = new SyncStreamBinaryReader(stream);
        IBinaryReader reader = binaryReader;

        (await reader.ReadAsync(0)).Should().BeEmpty();
    }

    [Test]
    public void ReadAsync_Truncated()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        using var binaryReader = new SyncStreamBinaryReader(stream);
        IBinaryReader reader = binaryReader;

        AssertThat.Invoking(() => reader.ReadAsync(4)).Should().Throw<EndOfStreamException>();
    }

    [Test]
    public async Task ReadToEndAsync()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        using var binaryReader = new SyncStreamBinaryReader(stream);
        IBinaryReader reader = binaryReader;

        (await reader.ReadAsync(2)).Should().SequenceEqual(1, 2);
        (await reader.ReadToEndAsync()).Should().SequenceEqual(3, 4, 5);
    }

    [Test]
    public async Task AtEndAsync()
    {
        using var stream = new MemoryStream([1, 2]);
        using var binaryReader = new SyncStreamBinaryReader(stream);
        IBinaryReader reader = binaryReader;

        (await reader.AtEndAsync()).Should().BeFalse();
        (await reader.ReadAsync(2)).Should().SequenceEqual(1, 2);
        (await reader.AtEndAsync()).Should().BeTrue();
    }

    [Test]
    public async Task AtEndAsync_DoesNotConsumeBytes()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        using var binaryReader = new SyncStreamBinaryReader(stream);
        IBinaryReader reader = binaryReader;

        // Peeking for the end of the stream must not consume the peeked byte.
        (await reader.AtEndAsync()).Should().BeFalse();
        (await reader.AtEndAsync()).Should().BeFalse();
        (await reader.ReadAsync(3)).Should().SequenceEqual(1, 2, 3);

        (await reader.AtEndAsync()).Should().BeTrue();
    }

    [Test]
    public async Task AtEndAsync_ThenReadToEndIncludesPeekedByte()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        using var binaryReader = new SyncStreamBinaryReader(stream);
        IBinaryReader reader = binaryReader;

        (await reader.AtEndAsync()).Should().BeFalse();
        (await reader.ReadToEndAsync()).Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Dispose_DoesNotCloseUnderlyingStream()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var binaryReader = new SyncStreamBinaryReader(stream);

        binaryReader.Dispose();

        stream.Invoking(s => s.ReadByte()).Should().NotThrow();
    }
}