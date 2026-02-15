using System.IO.Compression;

namespace MrKWatkins.OakIO;

public abstract class FileFormat(string name, string fileExtension)
{
    public string Name { get; } = name;

    public string FileExtension { get; } = fileExtension;

    [Pure]
    public string GetFilename(string name) => $"{name}.{FileExtension}";

    [Pure]
    public IOFile Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Read(stream);
    }

    [MustUseReturnValue]
    public abstract IOFile Read(Stream stream);

    public void Write(IOFile file, [PathReference] string directory, string name, bool zipped = false) => Write(file, Path.Combine(directory, GetFilename(name)), zipped);

    public void Write(IOFile file, [PathReference] string filePath, bool zipped = false)
    {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Extension != $".{FileExtension}")
        {
            throw new ArgumentException($"Value has the extension {fileInfo.Extension} rather than the expected .{FileExtension}.", nameof(filePath));
        }

        var filename = fileInfo.Name;
        using var stream = File.Create(Path.Combine(fileInfo.DirectoryName!, zipped ? $"{filename}.zip" : filename));
        if (zipped)
        {
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create);
            var entry = zip.CreateEntry(filename);
            using var entryStream = entry.Open();
            Write(file, entryStream);
        }
        else
        {
            Write(file, stream);
        }
    }

    [Pure]
    public byte[] Write(IOFile file)
    {
        using var memoryStream = new MemoryStream();
        Write(file, memoryStream);
        return memoryStream.ToArray();
    }

    public abstract void Write(IOFile file, Stream stream);
}

public abstract class FileFormat<TFile>(string name, string fileExtension) : FileFormat(name, fileExtension)
    where TFile : IOFile
{
    public sealed override void Write(IOFile file, Stream stream)
    {
        if (file is not TFile typedFile)
        {
            throw new ArgumentException($"Value is not of type {typeof(TFile).Name}.", nameof(file));
        }

        Write(typedFile, stream);
    }

    protected abstract void Write(TFile file, Stream stream);
}