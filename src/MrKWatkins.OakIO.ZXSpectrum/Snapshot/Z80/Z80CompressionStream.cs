using System.IO.Compression;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

// Details of the Z80 format's compression can be found at https://worldofspectrum.org/faq/reference/z80format.htm.
public sealed class Z80CompressionStream : Stream
{
    private readonly Stream stream;
    private readonly bool leaveOpen;
    private readonly Decompressor? decompressor;
    private readonly Compressor? compressor;
    private bool disposed;

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

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        VerifyNotDisposed();
        VerifyDecompressing();
        return decompressor.Read(stream, buffer);
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        VerifyNotDisposed();
        VerifyCompressing();
        compressor.Write(stream, buffer);
    }

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

    public override bool CanRead => decompressor != null;

    public override bool CanWrite => compressor != null;

    public override void Flush()
    {
    }

    public override bool CanSeek => false;

    public override long Length => throw new NotSupportedException($"{nameof(Length)} is not supported.");

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException($"{nameof(Seek)} is not supported.");

    public override void SetLength(long value) => throw new NotSupportedException($"{nameof(SetLength)} is not supported.");

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