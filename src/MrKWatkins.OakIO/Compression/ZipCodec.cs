using System.IO.Compression;

namespace MrKWatkins.OakIO.Compression;

internal sealed class ZipCodec : Codec
{
    public static readonly ZipCodec Instance = new();

    private ZipCodec()
    {
    }

    protected override IOFile Read([PathReference] string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats)
    {
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read, true);
        foreach (var entry in zip.Entries)
        {
            if (TryGetFileFormat(entry.Name, supportedFormats, out var format))
            {
                using var entryStream = entry.Open();
                return format.Read(entryStream);
            }
        }
        throw new NotSupportedException("No file found in ZIP archive of a supported format.");
    }

    protected override async Task<IOFile> ReadAsync(string path, Stream stream, IReadOnlyList<IOFileFormat> supportedFormats, CancellationToken cancellationToken)
    {
        var zip = new ZipArchive(stream, ZipArchiveMode.Read, true);
        await using (zip.ConfigureAwait(false))
        {
            foreach (var entry in zip.Entries)
            {
                if (TryGetFileFormat(entry.Name, supportedFormats, out var format))
                {
                    var entryStream = await entry.OpenAsync(cancellationToken).ConfigureAwait(false);
                    await using (entryStream.ConfigureAwait(false))
                    {
                        return await format.ReadAsync(entryStream, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        throw new NotSupportedException("No file found in ZIP archive of a supported format.");
    }

    protected override string GetCompressedFilename(string filenameWithFormatExtension) => Path.ChangeExtension(filenameWithFormatExtension, "zip");

    protected override void Write(IOFile file, Stream stream, string filenameWithFormatExtension)
    {
        using var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
        var entry = zip.CreateEntry(filenameWithFormatExtension);
        using var entryStream = entry.Open();
        file.Write(entryStream);
    }

    protected override async Task WriteAsync(IOFile file, Stream stream, string filenameWithFormatExtension, CancellationToken cancellationToken)
    {
        var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
        await using (zip.ConfigureAwait(false))
        {
            var entry = zip.CreateEntry(filenameWithFormatExtension);
            var entryStream = await entry.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using (entryStream.ConfigureAwait(false))
            {
                await file.WriteAsync(entryStream, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}