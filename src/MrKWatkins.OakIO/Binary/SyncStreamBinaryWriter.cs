using System.Buffers;

namespace MrKWatkins.OakIO.Binary;

internal sealed class SyncStreamBinaryWriter(Stream stream) : IBinaryWriter, IDisposable
{
    private readonly ArrayBufferWriter<byte> buffer = new();

    public Span<byte> GetSpan(int sizeHint = 0) => buffer.GetSpan(sizeHint);

    public Memory<byte> GetMemory(int sizeHint = 0) => buffer.GetMemory(sizeHint);

    public void Advance(int count) => buffer.Advance(count);

    public ValueTask WriteAsync(ReadOnlyMemory<byte> value)
    {
        // Drain any buffered data.
#pragma warning disable CA1849
        Flush();
#pragma warning restore CA1849

        // Zero-copy pass through the payload.
        stream.Write(value.Span);

        return ValueTask.CompletedTask;
    }

    public ValueTask FlushAsync()
    {
        Flush();
        return ValueTask.CompletedTask;
    }

    private void Flush()
    {
        if (buffer.WrittenCount == 0)
        {
            return;
        }

        stream.Write(buffer.WrittenSpan);
        // Keep the capacity but reset the position.
        buffer.ResetWrittenCount();
    }

    public void Dispose() => Flush();
}