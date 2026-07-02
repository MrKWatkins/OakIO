namespace MrKWatkins.OakIO.Binary;

internal sealed class AsyncStreamBinaryWriter(Stream stream, CancellationToken cancellationToken) : IBinaryWriter, IAsyncDisposable
{
    public ValueTask WriteAsync(ReadOnlyMemory<byte> value) => stream.WriteAsync(value, cancellationToken);

    public ValueTask DisposeAsync() => new(stream.FlushAsync(cancellationToken));
}