namespace MrKWatkins.OakIO.ZXSpectrum;

public abstract class SnapshotFormat(string name, string fileExtension) : FileFormat(name, fileExtension)
{
    [Pure]
    public new SnapshotFile Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Read(stream);
    }

    public sealed override SnapshotFile Read(Stream stream) => ReadSnapshot(stream);

    [MustUseReturnValue]
    protected abstract SnapshotFile ReadSnapshot(Stream stream);
}

public abstract class SnapshotFormat<TFile>(string name, string fileExtension) : SnapshotFormat(name, fileExtension)
    where TFile : SnapshotFile
{
    [Pure]
    public new TFile Read(byte[] bytes) => (TFile)base.Read(bytes);

    [MustUseReturnValue]
    public new TFile Read(Stream stream) => (TFile)base.Read(stream);

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