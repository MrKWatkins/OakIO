using MrKWatkins.OakIO.Compression;

namespace MrKWatkins.OakIO;

/// <summary>
/// Base class for a file of a given format.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class IOFile
{
    /// <summary>
    /// Initialises a new instance of the <see cref="IOFile" /> class.
    /// </summary>
    /// <param name="format">The format of the file.</param>
    protected IOFile(IOFileFormat format)
    {
        format.EnsureConvertersAreRegistered();
        Format = format;
    }

    /// <summary>
    /// Gets the format of this file.
    /// </summary>
    public IOFileFormat Format { get; }

    /// <summary>
    /// Saves this file to a directory with the specified name.
    /// </summary>
    /// <param name="directory">The directory to write the file to.</param>
    /// <param name="name">The name of the file without extension.</param>
    /// <param name="compressionFormat">Optional compression format to use.</param>
    /// <returns>The full path of the written file.</returns>
    public string Save([PathReference] string directory, string name, CompressionFormat compressionFormat = CompressionFormat.None) => Codec.Save(this, directory, name, compressionFormat);

    /// <summary>
    /// Saves this file to a directory with the specified name asynchronously
    /// </summary>
    /// <param name="directory">The directory to write the file to.</param>
    /// <param name="name">The name of the file without extension.</param>
    /// <param name="compressionFormat">Optional compression format to use.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the save.</param>
    /// <returns>The full path of the written file.</returns>
    public Task<string> SaveAsync([PathReference] string directory, string name, CompressionFormat compressionFormat = CompressionFormat.None, CancellationToken cancellationToken = default) =>
        Codec.SaveAsync(this, directory, name, compressionFormat, cancellationToken);

    /// <summary>
    /// Writes this file to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public void Write(Stream stream) => Format.Write(this, stream);

    /// <summary>
    /// Writes this file to a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="cancellationToken">An <see cref="CancellationToken"/> to cancel the writing.</param>
    public Task WriteAsync(Stream stream, CancellationToken cancellationToken = default) => Format.WriteAsync(this, stream, cancellationToken);

    /// <summary>
    /// Converts this file to a byte array.
    /// </summary>
    /// <returns>A byte array containing the file data.</returns>
    [Pure]
    public byte[] ToByteArray()
    {
        using var memoryStream = new MemoryStream();
        Format.Write(this, memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Attempts to load the file data into the specified memory span.
    /// </summary>
    /// <param name="memory">The memory span to load the data into.</param>
    /// <returns><c>true</c> if the data was loaded successfully; <c>false</c> otherwise.</returns>
    [MustUseReturnValue]
    public virtual bool TryLoadInto(Span<byte> memory) => false;

    /// <summary>
    /// Loads the file data into the specified memory span.
    /// </summary>
    /// <param name="memory">The memory span to load the data into.</param>
    public virtual void LoadInto(Span<byte> memory)
    {
        if (!TryLoadInto(memory))
        {
            throw new IOException($"{Format.Name} files cannot be loaded into memory.");
        }
    }
}