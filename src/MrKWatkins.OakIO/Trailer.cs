namespace MrKWatkins.OakIO;

/// <summary>
/// Base class for a trailer in a file, either for the file as a whole or a block within the file.
/// </summary>
public abstract class Trailer : IOFileComponent
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Trailer" /> class with zero-filled data of the specified length.
    /// </summary>
    /// <param name="length">The length of the trailer in bytes.</param>
    protected Trailer(int length) : base(length)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Trailer" /> class by reading data from a stream.
    /// </summary>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="data">The stream to read the trailer data from.</param>
    protected Trailer(int length, Stream data) : base(length, data)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Trailer" /> class from a sequence of bytes.
    /// </summary>
    /// <param name="length">The expected length of the trailer in bytes.</param>
    /// <param name="data">The bytes for this trailer.</param>
    protected Trailer(int length, IEnumerable<byte> data) : base(length, data)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Trailer" /> class from a byte array.
    /// </summary>
    /// <param name="data">The raw byte data for this trailer.</param>
    protected Trailer(byte[] data) : base(data)
    {
    }
}