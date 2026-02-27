namespace MrKWatkins.OakIO;

/// <summary>
/// Base class for a header in a file, either for the file as a whole or a block within the file.
/// </summary>
public abstract class Header : IOFileComponent
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Header" /> class with zero-filled data of the specified length.
    /// </summary>
    /// <param name="length">The length of the header in bytes.</param>
    protected Header(int length)
        : base(length)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Header" /> class by reading data from a stream.
    /// </summary>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="data">The stream to read the header data from.</param>
    protected Header(int length, Stream data)
        : base(length, data)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Header" /> class from a sequence of bytes.
    /// </summary>
    /// <param name="length">The expected length of the header in bytes.</param>
    /// <param name="data">The bytes for this header.</param>
    protected Header(int length, [InstantHandle] IEnumerable<byte> data)
        : base(length, data)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Header" /> class from a byte array.
    /// </summary>
    /// <param name="data">The raw byte data for this header.</param>
    protected Header(byte[] data)
        : base(data)
    {
    }
}