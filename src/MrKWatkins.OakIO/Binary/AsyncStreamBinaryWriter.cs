using System.IO.Pipelines;

namespace MrKWatkins.OakIO.Binary;

internal sealed class AsyncStreamBinaryWriter(Stream stream, CancellationToken cancellationToken) : IBinaryWriter, IAsyncDisposable
{
    private readonly PipeWriter pipe = PipeWriter.Create(stream, new StreamPipeWriterOptions(leaveOpen: true));

    public Span<byte> GetSpan(int sizeHint = 0) => pipe.GetSpan(sizeHint);

    public Memory<byte> GetMemory(int sizeHint = 0) => pipe.GetMemory(sizeHint);

    public void Advance(int count) => pipe.Advance(count);

    public async ValueTask WriteAsync(ReadOnlyMemory<byte> value)
    {
        // Drain any buffered data.
        await pipe.FlushAsync(cancellationToken).ConfigureAwait(false);

        // Zero-copy pass through the payload.
        await stream.WriteAsync(value, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask FlushAsync() => await pipe.FlushAsync(cancellationToken).ConfigureAwait(false);

    public ValueTask DisposeAsync() => pipe.CompleteAsync();
}