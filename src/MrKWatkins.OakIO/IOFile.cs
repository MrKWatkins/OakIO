using System.Diagnostics;
using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Compression;

namespace MrKWatkins.OakIO;

/// <summary>
/// Base class for a file of a given format.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class IOFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IOFile" /> class.
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
    public void Write(Stream stream)
    {
        using var writer = new SyncStreamBinaryWriter(stream);
        var write = Format.WriteAsync(this, writer);
        Debug.Assert(write.IsCompleted, $"{nameof(SyncStreamBinaryWriter)} must complete synchronously.");
        write.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Writes this file to a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="cancellationToken">An <see cref="CancellationToken"/> to cancel the writing.</param>
    public async Task WriteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var writer = new AsyncStreamBinaryWriter(stream, cancellationToken);
        await using (writer.ConfigureAwait(false))
        {
            await Format.WriteAsync(this, writer).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Writes this file to a stream, optionally compressing it.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="filename">The filename of the file, with extension. Used as the name of the entry when <paramref name="compressionFormat" /> is <see cref="CompressionFormat.Zip" />.</param>
    /// <param name="compressionFormat">The compression format to use.</param>
    public void Write(Stream stream, string filename, CompressionFormat compressionFormat) => Codec.Write(this, stream, filename, compressionFormat);

    /// <summary>
    /// Writes this file to a stream asynchronously, optionally compressing it.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="filename">The filename of the file, with extension. Used as the name of the entry when <paramref name="compressionFormat" /> is <see cref="CompressionFormat.Zip" />.</param>
    /// <param name="compressionFormat">The compression format to use.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the writing.</param>
    public Task WriteAsync(Stream stream, string filename, CompressionFormat compressionFormat, CancellationToken cancellationToken = default) =>
        Codec.WriteAsync(this, stream, filename, compressionFormat, cancellationToken);

    /// <summary>
    /// Gets the filename to use for a file written with the specified compression format, for example <c>"name.wav"</c>
    /// compressed with <see cref="CompressionFormat.GZip" /> becomes <c>"name.wav.gz"</c>.
    /// </summary>
    /// <param name="filename">The filename of the uncompressed file, with extension.</param>
    /// <param name="compressionFormat">The compression format.</param>
    /// <returns>The filename to use for the compressed file.</returns>
    [Pure]
    public static string GetCompressedFilename(string filename, CompressionFormat compressionFormat) => Codec.GetCompressedFilename(filename, compressionFormat);

    /// <summary>
    /// Converts this file to a byte array.
    /// </summary>
    /// <returns>A byte array containing the file data.</returns>
    [Pure]
    public byte[] ToByteArray()
    {
        // TODO: Straight to array?
        using var memoryStream = new MemoryStream();
        Write(memoryStream);
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