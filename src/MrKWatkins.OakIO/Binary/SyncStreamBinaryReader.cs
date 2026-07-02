using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.Binary;

internal sealed class SyncStreamBinaryReader(Stream stream) : IBinaryReader, IDisposable
{
    // leaveOpen defaults to true, so disposing this does not close the underlying stream.
    private readonly PeekableStream peekable = new(stream);

    public ValueTask<byte[]> ReadAsync(int count) => new(peekable.ReadExactly(count));

    public ValueTask<byte[]> ReadToEndAsync() => new(peekable.ReadAllBytes());

    public ValueTask<bool> AtEndAsync() => new(peekable.EndOfStream);

    public void Dispose() => peekable.Dispose();
}