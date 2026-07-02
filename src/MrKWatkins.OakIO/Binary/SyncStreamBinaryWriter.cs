namespace MrKWatkins.OakIO.Binary;

internal sealed class SyncStreamBinaryWriter(Stream stream) : IBinaryWriter, IDisposable
{
    public ValueTask WriteAsync(ReadOnlyMemory<byte> value)
    {
        stream.Write(value.Span);
        return ValueTask.CompletedTask;
    }

    public void Dispose() => stream.Flush();
}