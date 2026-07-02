using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Tests.Binary;

public sealed class AsyncStreamBinaryReaderTests
{
    [Test]
    public async Task ReadAsync()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        await using var binaryReader = new AsyncStreamBinaryReader(stream, CancellationToken.None);
        IBinaryReader reader = binaryReader;

        (await reader.ReadAsync(2)).Should().SequenceEqual(1, 2);
        (await reader.ReadAsync(3)).Should().SequenceEqual(3, 4, 5);
    }

    [Test]
    public async Task ReadAsync_Zero()
    {
        using var stream = new MemoryStream([1]);
        await using var binaryReader = new AsyncStreamBinaryReader(stream, CancellationToken.None);
        IBinaryReader reader = binaryReader;

        (await reader.ReadAsync(0)).Should().BeEmpty();
    }

    [Test]
    public async Task ReadAsync_ZeroAfterPeek()
    {
        using var stream = new MemoryStream([1, 2]);
        await using var binaryReader = new AsyncStreamBinaryReader(stream, CancellationToken.None);
        IBinaryReader reader = binaryReader;

        (await reader.AtEndAsync()).Should().BeFalse();     // Reads ahead one byte.
        (await reader.ReadAsync(0)).Should().BeEmpty();     // Must not lose the read-ahead byte.
        (await reader.ReadAsync(2)).Should().SequenceEqual(1, 2);
    }

    [Test]
    public async Task ReadAsync_Truncated()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        await using var binaryReader = new AsyncStreamBinaryReader(stream, CancellationToken.None);
        IBinaryReader reader = binaryReader;

        await reader.Awaiting(r => r.ReadAsync(4).AsTask()).Should().ThrowAsync<EndOfStreamException>();
    }

    [Test]
    public async Task ReadToEndAsync()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        await using var binaryReader = new AsyncStreamBinaryReader(stream, CancellationToken.None);
        IBinaryReader reader = binaryReader;

        (await reader.ReadAsync(2)).Should().SequenceEqual(1, 2);
        (await reader.ReadToEndAsync()).Should().SequenceEqual(3, 4, 5);
    }

    [Test]
    public async Task AtEndAsync()
    {
        using var stream = new MemoryStream([1, 2]);
        await using var binaryReader = new AsyncStreamBinaryReader(stream, CancellationToken.None);
        IBinaryReader reader = binaryReader;

        (await reader.AtEndAsync()).Should().BeFalse();
        (await reader.ReadAsync(2)).Should().SequenceEqual(1, 2);
        (await reader.AtEndAsync()).Should().BeTrue();
    }

    [Test]
    public async Task AtEndAsync_DoesNotConsumeBytes()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        await using var binaryReader = new AsyncStreamBinaryReader(stream, CancellationToken.None);
        IBinaryReader reader = binaryReader;

        // Reading ahead to detect the end of the stream must not consume the byte.
        (await reader.AtEndAsync()).Should().BeFalse();
        (await reader.AtEndAsync()).Should().BeFalse();
        (await reader.ReadAsync(3)).Should().SequenceEqual(1, 2, 3);

        (await reader.AtEndAsync()).Should().BeTrue();
    }

    [Test]
    public async Task AtEndAsync_ThenReadToEndIncludesPeekedByte()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        await using var binaryReader = new AsyncStreamBinaryReader(stream, CancellationToken.None);
        IBinaryReader reader = binaryReader;

        (await reader.AtEndAsync()).Should().BeFalse();
        (await reader.ReadToEndAsync()).Should().SequenceEqual(1, 2, 3);
    }
}