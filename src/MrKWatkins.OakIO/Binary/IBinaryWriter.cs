using System.Buffers;
using Primitives = System.Buffers.Binary.BinaryPrimitives;

namespace MrKWatkins.OakIO.Binary;

/// <summary>
/// Writes primitive values to an underlying destination. Small values are written synchronously into a buffer via the
/// <see cref="IBufferWriter{T}" /> members; larger payloads and flushing are performed via <see cref="WriteAsync" /> and
/// <see cref="FlushAsync" />. Implementations may complete these operations either synchronously or asynchronously.
/// </summary>
public interface IBinaryWriter : IBufferWriter<byte>
{
    /// <summary>
    /// Writes a single byte.
    /// </summary>
    /// <param name="value">The byte to write.</param>
    public void WriteByte(byte value)
    {
        var span = GetSpan(1);
        span[0] = value;
        Advance(1);
    }

    /// <summary>
    /// Writes an unsigned 16-bit integer in little-endian byte order.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteUInt16LittleEndian(ushort value)
    {
        Primitives.WriteUInt16LittleEndian(GetSpan(2), value);
        Advance(2);
    }

    /// <summary>
    /// Writes a signed 16-bit integer in little-endian byte order.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteInt16LittleEndian(short value)
    {
        Primitives.WriteInt16LittleEndian(GetSpan(2), value);
        Advance(2);
    }

    /// <summary>
    /// Writes an unsigned 32-bit integer in little-endian byte order.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteUInt32LittleEndian(uint value)
    {
        Primitives.WriteUInt32LittleEndian(GetSpan(4), value);
        Advance(4);
    }

    /// <summary>
    /// Writes an unsigned 32-bit integer in big-endian byte order.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteUInt32BigEndian(uint value)
    {
        Primitives.WriteUInt32BigEndian(GetSpan(4), value);
        Advance(4);
    }

    /// <summary>
    /// Writes a signed 32-bit integer in little-endian byte order.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteInt32LittleEndian(int value)
    {
        Primitives.WriteInt32LittleEndian(GetSpan(4), value);
        Advance(4);
    }

    /// <summary>
    /// Writes a sequence of bytes into the buffer.
    /// </summary>
    /// <param name="value">The bytes to write.</param>
    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        value.CopyTo(GetSpan(value.Length));
        Advance(value.Length);
    }

    /// <summary>
    /// Flushes any buffered data and then writes the specified payload straight to the underlying destination, bypassing
    /// the buffer. Intended for large payloads that should not be copied into the buffer.
    /// </summary>
    /// <param name="value">The bytes to write.</param>
    /// <returns>A <see cref="ValueTask" /> that completes when the payload has been written.</returns>
    ValueTask WriteAsync(ReadOnlyMemory<byte> value);

    /// <summary>
    /// Flushes any buffered data to the underlying destination.
    /// </summary>
    /// <returns>A <see cref="ValueTask" /> that completes when the buffered data has been flushed.</returns>
    ValueTask FlushAsync();
}