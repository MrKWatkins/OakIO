namespace MrKWatkins.OakIO.Binary;

/// <summary>
/// Writes binary data to an underlying destination as byte arrays. Values should be built up as byte arrays using the
/// <c>MrKWatkins.BinaryPrimitives</c> extension methods and written in one go. Implementations may complete the write
/// either synchronously or asynchronously.
/// </summary>
public interface IBinaryWriter
{
    /// <summary>
    /// Writes the specified bytes to the underlying destination.
    /// </summary>
    /// <param name="value">The bytes to write.</param>
    /// <returns>A <see cref="ValueTask" /> that completes when the bytes have been written.</returns>
    ValueTask WriteAsync(ReadOnlyMemory<byte> value);
}