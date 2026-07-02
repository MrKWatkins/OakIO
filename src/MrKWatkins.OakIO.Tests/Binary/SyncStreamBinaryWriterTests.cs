using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Tests.Binary;

public sealed class SyncStreamBinaryWriterTests
{
    [Test]
    public async Task WriteAsync()
    {
        using var stream = new MemoryStream();

        using (var binaryWriter = new SyncStreamBinaryWriter(stream))
        {
            IBinaryWriter writer = binaryWriter;

            var write = writer.WriteAsync(new byte[] { 0x01, 0x02 });
            write.IsCompleted.Should().BeTrue();    // The synchronous writer completes synchronously.
            await write;

            await writer.WriteAsync(new byte[] { 0x03 });
        }

        stream.ToArray().Should().SequenceEqual(0x01, 0x02, 0x03);
    }
}