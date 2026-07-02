using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Tests.Binary;

public sealed class AsyncStreamBinaryWriterTests
{
    [Test]
    public async Task WriteAsync()
    {
        using var stream = new MemoryStream();

        var binaryWriter = new AsyncStreamBinaryWriter(stream, CancellationToken.None);
        await using (binaryWriter)
        {
            IBinaryWriter writer = binaryWriter;

            await writer.WriteAsync(new byte[] { 0x01, 0x02 });
            await writer.WriteAsync(new byte[] { 0x03 });
        }

        stream.ToArray().Should().SequenceEqual(0x01, 0x02, 0x03);
    }
}