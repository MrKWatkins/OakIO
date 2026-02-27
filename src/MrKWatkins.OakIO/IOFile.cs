using System.IO.Compression;

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
    /// Reads a file from disk.
    /// </summary>
    /// <param name="filename">The path to the file to read.</param>
    /// <param name="possibleFormats">The possible formats the file could be in.</param>
    /// <returns>The file that was read.</returns>
    [Pure]
    public static IOFile Read([PathReference] string filename, params IReadOnlyList<IOFileFormat> possibleFormats)
    {
        using var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Read(filename, file, possibleFormats);
    }

    /// <summary>
    /// Reads a file from a stream.
    /// </summary>
    /// <param name="filename">The filename, used to determine the format from the extension.</param>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="possibleFormats">The possible formats the file could be in.</param>
    /// <returns>The file that was read.</returns>
    [MustUseReturnValue]
    public static IOFile Read([PathReference] string filename, Stream stream, params IReadOnlyList<IOFileFormat> possibleFormats)
    {
        var extension = GetExtension(filename);
        return extension == ".zip"
            ? ReadZip(stream, possibleFormats)
            : GetFormat(extension, possibleFormats).Read(stream);
    }

    [MustUseReturnValue]
    private static IOFile ReadZip(Stream stream, IReadOnlyList<IOFileFormat> possibleFormats)
    {
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read, true);
        foreach (var entry in zip.Entries)
        {
            var format = GetFormatOrNull(GetExtension(entry.Name), possibleFormats);
            if (format != null)
            {
                using var entryStream = entry.Open();
                return format.Read(entryStream);
            }
        }

        throw new NotSupportedException("No file found in ZIP archive of a supported format.");
    }

    /// <summary>
    /// Writes this file to disk.
    /// </summary>
    /// <param name="filePath">The path to write the file to.</param>
    /// <param name="zipped">Whether to write the file inside a ZIP archive.</param>
    public void Write([PathReference] string filePath, bool zipped = false) => Format.Write(this, filePath, zipped);

    /// <summary>
    /// Writes this file to a directory with the specified name.
    /// </summary>
    /// <param name="directory">The directory to write the file to.</param>
    /// <param name="name">The name of the file without extension.</param>
    /// <param name="zipped">Whether to write the file inside a ZIP archive.</param>
    public void Write([PathReference] string directory, string name, bool zipped = false) => Format.Write(this, directory, name, zipped);

    /// <summary>
    /// Writes this file to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public void Write(Stream stream) => Format.Write(this, stream);

    /// <summary>
    /// Writes this file to a byte array.
    /// </summary>
    /// <returns>A byte array containing the file data.</returns>
    [Pure]
    public byte[] Write() => Format.Write(this);

    [Pure]
    private static IOFileFormat GetFormat(string extension, IReadOnlyList<IOFileFormat> possibleFormats) =>
        GetFormatOrNull(extension, possibleFormats) ?? throw new NotSupportedException($"The file extension \"{extension}\" is not supported.");

    [Pure]
    private static IOFileFormat? GetFormatOrNull(string extension, IReadOnlyList<IOFileFormat> possibleFormats)
    {
        extension = extension[1..];
        return possibleFormats.FirstOrDefault(f => f.FileExtension == extension);
    }

    [Pure]
    private static string GetExtension(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return string.IsNullOrWhiteSpace(extension) ? throw new ArgumentException("Value has no extension.", nameof(filename)) : extension;
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