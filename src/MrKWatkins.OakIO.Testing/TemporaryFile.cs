using System.IO.Compression;

namespace MrKWatkins.OakIO.Testing;

public sealed class TemporaryFile : IDisposable
{
    private readonly TemporaryDirectory tempDirectory;

    private TemporaryFile(TemporaryDirectory tempDirectory, string path)
    {
        this.tempDirectory = tempDirectory;
        Path = path;
    }

    [Pure]
    [MustDisposeResource]
    public static TemporaryFile Create(string? filename = null, bool zipped = false) => Create([], filename, zipped);

    [Pure]
    [MustDisposeResource]
    public static TemporaryFile Create(ReadOnlySpan<byte> contents, string? filename = null, bool zipped = false)
    {
        using var stream = new MemoryStream();
        stream.Write(contents);
        stream.Position = 0;

        return Create(stream, filename, zipped);
    }

    [MustUseReturnValue]
    [MustDisposeResource]
    public static TemporaryFile Create(Stream contents, string? filename = null, bool zipped = false)
    {
        var tempDirectory = TemporaryDirectory.Create();

        filename ??= $"{Guid.NewGuid()}.tmp";

        var path = tempDirectory.GetFilePath(filename);
        if (zipped)
        {
            path += ".zip";
        }

        using var stream = File.Create(path);
        if (zipped)
        {
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create);
            var entry = zip.CreateEntry(filename);
            using var entryStream = entry.Open();
            contents.CopyTo(entryStream);
        }
        else
        {
            contents.CopyTo(stream);
        }

        return new TemporaryFile(tempDirectory, path);
    }

    public string Path { get; }

    [Pure]
    public string Name => new FileInfo(Path).Name;

    [Pure]
    [MustDisposeResource]
    public Stream OpenRead() => File.OpenRead(Path);

    [Pure]
    public byte[] Bytes => File.ReadAllBytes(Path);

    [Pure]
    [MustDisposeResource]
    public Stream OpenWrite() => File.OpenWrite(Path);

    public void Dispose() => tempDirectory.Dispose();
}