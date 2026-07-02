namespace MrKWatkins.OakIO.Binary;

/// <summary>
/// Reads binary data from an underlying source as byte arrays. Values should be read in whole blocks and then picked
/// apart using the <c>MrKWatkins.BinaryPrimitives</c> extension methods. Implementations may complete reads either
/// synchronously or asynchronously.
/// </summary>
public interface IBinaryReader
{
    /// <summary>
    /// Reads exactly <paramref name="count" /> bytes from the source.
    /// </summary>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>A <see cref="ValueTask{TResult}" /> yielding the bytes that were read.</returns>
    /// <exception cref="EndOfStreamException">The source ended before <paramref name="count" /> bytes could be read.</exception>
    [MustUseReturnValue]
    ValueTask<byte[]> ReadAsync(int count);

    /// <summary>
    /// Reads all remaining bytes from the source.
    /// </summary>
    /// <returns>A <see cref="ValueTask{TResult}" /> yielding the remaining bytes.</returns>
    [MustUseReturnValue]
    ValueTask<byte[]> ReadToEndAsync();

    /// <summary>
    /// Determines whether the end of the source has been reached.
    /// </summary>
    /// <returns>A <see cref="ValueTask{TResult}" /> yielding <c>true</c> if there are no more bytes to read; <c>false</c> otherwise.</returns>
    ValueTask<bool> AtEndAsync();
}