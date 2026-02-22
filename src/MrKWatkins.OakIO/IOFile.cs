using System.IO.Compression;

namespace MrKWatkins.OakIO;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class IOFile
{
    protected IOFile(IOFileFormat format)
    {
        format.EnsureConvertersAreRegistered();
        Format = format;
    }

    public IOFileFormat Format { get; }

    [Pure]
    public static IOFile Read([PathReference] string filename, params IReadOnlyList<IOFileFormat> possibleFormats)
    {
        using var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Read(filename, file, possibleFormats);
    }

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

    public void Write([PathReference] string filePath, bool zipped = false) => Format.Write(this, filePath, zipped);

    public void Write([PathReference] string directory, string name, bool zipped = false) => Format.Write(this, directory, name, zipped);

    public void Write(Stream stream) => Format.Write(this, stream);

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

    [MustUseReturnValue]
    public virtual bool TryLoadInto(Span<byte> memory) => false;

    public virtual void LoadInto(Span<byte> memory)
    {
        if (!TryLoadInto(memory))
        {
            throw new IOException($"{Format.Name} files cannot be loaded into memory.");
        }
    }
}