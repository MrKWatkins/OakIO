using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Tests.Binary;

public sealed class AsyncStreamBinaryWriterTests
{
    [Test]
    public async Task Writes()
    {
        using var stream = new MemoryStream();

        var binaryWriter = new AsyncStreamBinaryWriter(stream, CancellationToken.None);
        await using (binaryWriter)
        {
            IBinaryWriter writer = binaryWriter;

            writer.WriteByte(0xAB);
            writer.WriteUInt16LittleEndian(0x1234);
            writer.WriteInt16LittleEndian(-2);
            writer.WriteUInt32LittleEndian(0x12345678);
            writer.WriteInt32LittleEndian(-1);
            writer.WriteUInt32BigEndian(0x12345678);
            writer.WriteBytes([0x01, 0x02]);

            // Explicitly flush the buffered data, then pass a payload straight through.
            await writer.FlushAsync();
            await writer.WriteAsync(new byte[] { 0x03, 0x04 });
        }

        stream.ToArray().Should().SequenceEqual(
            0xAB,                       // WriteByte
            0x34, 0x12,                 // WriteUInt16LittleEndian
            0xFE, 0xFF,                 // WriteInt16LittleEndian (-2)
            0x78, 0x56, 0x34, 0x12,     // WriteUInt32LittleEndian
            0xFF, 0xFF, 0xFF, 0xFF,     // WriteInt32LittleEndian (-1)
            0x12, 0x34, 0x56, 0x78,     // WriteUInt32BigEndian
            0x01, 0x02,                 // WriteBytes
            0x03, 0x04                  // WriteAsync
        );
    }

    [Test]
    public async Task GetMemory()
    {
        using var stream = new MemoryStream();

        var binaryWriter = new AsyncStreamBinaryWriter(stream, CancellationToken.None);
        await using (binaryWriter)
        {
            IBinaryWriter writer = binaryWriter;

            var memory = writer.GetMemory(2);
            memory.Span[0] = 0xAA;
            memory.Span[1] = 0xBB;
            writer.Advance(2);
        }

        stream.ToArray().Should().SequenceEqual(0xAA, 0xBB);
    }
}