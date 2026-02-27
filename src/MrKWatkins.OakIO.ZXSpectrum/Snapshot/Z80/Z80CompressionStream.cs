using System.IO.Compression;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

// Details of the Z80 format's compression can be found at https://worldofspectrum.org/faq/reference/z80format.htm.
/// <summary>
/// A stream that compresses or decompresses data using the Z80 snapshot compression format.
/// </summary>
public sealed class Z80CompressionStream : Stream
{
    private readonly Stream stream;
    private readonly bool leaveOpen;
    private readonly Decompressor? decompressor;
    private readonly Compressor? compressor;
    private bool disposed;

    /// <summary>
    /// Initialises a new instance of the <see cref="Z80CompressionStream" /> class.
    /// </summary>
    /// <param name="stream">The underlying stream to read from or write to.</param>
    /// <param name="mode">Whether to compress or decompress.</param>
    /// <param name="endMarker"><c>true</c> to use an end marker; <c>false</c> otherwise.</param>
    /// <param name="leaveOpen"><c>true</c> to leave the underlying stream open; <c>false</c> otherwise.</param>
    public Z80CompressionStream(Stream stream, CompressionMode mode, bool endMarker = true, bool leaveOpen = true)
    {
        this.stream = stream;
        if (mode == CompressionMode.Decompress)
        {
            decompressor = new Decompressor(endMarker);
        }
        else
        {
            compressor = new Compressor(endMarker);
        }
        this.leaveOpen = leaveOpen;
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        VerifyNotDisposed();
        VerifyDecompressing();
        return decompressor.Read(stream, buffer);
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        VerifyNotDisposed();
        VerifyCompressing();
        compressor.Write(stream, buffer);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            disposed = true;
            compressor?.Close(stream);
            if (!leaveOpen)
            {
                stream.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override bool CanRead => decompressor != null;

    /// <inheritdoc />
    public override bool CanWrite => compressor != null;

    /// <inheritdoc />
    public override void Flush()
    {
    }

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException($"{nameof(Length)} is not supported.");

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException($"{nameof(Seek)} is not supported.");

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException($"{nameof(SetLength)} is not supported.");

    /// <inheritdoc />
    public override long Position
    {
        get => throw new NotSupportedException($"{nameof(Position)} is not supported.");
        set => throw new NotSupportedException($"{nameof(Position)} is not supported.");
    }

    [MemberNotNull(nameof(decompressor))]
    private void VerifyDecompressing()
    {
        if (decompressor == null)
        {
            throw new InvalidOperationException("This stream is for decompressing only.");
        }
    }

    [MemberNotNull(nameof(compressor))]
    private void VerifyCompressing()
    {
        if (compressor == null)
        {
            throw new InvalidOperationException("This stream is for compressing only.");
        }
    }

    private void VerifyNotDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Z80CompressionStream));
        }
    }
}