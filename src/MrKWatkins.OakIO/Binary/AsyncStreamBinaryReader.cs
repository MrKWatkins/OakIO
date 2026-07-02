using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.Binary;

internal sealed class AsyncStreamBinaryReader(Stream stream, CancellationToken cancellationToken) : IBinaryReader, IAsyncDisposable
{
    // leaveOpen defaults to true, so disposing this does not close the underlying stream.
    private readonly PeekableStream peekable = new(stream);

    public async ValueTask<byte[]> ReadAsync(int count)
    {
        var buffer = new byte[count];
        await peekable.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        return buffer;
    }

    public ValueTask<byte[]> ReadToEndAsync() => peekable.ReadAllBytesAsync(cancellationToken);

    public ValueTask<bool> AtEndAsync() => peekable.EndOfStreamAsync(cancellationToken);

    public ValueTask DisposeAsync() => peekable.DisposeAsync();
}