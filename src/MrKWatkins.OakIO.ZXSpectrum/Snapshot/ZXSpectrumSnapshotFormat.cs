namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot;

public abstract class ZXSpectrumSnapshotFormat(string name, string fileExtension, Type fileType) : IOFileFormat(name, fileExtension, fileType)
{
    [Pure]
    public new ZXSpectrumSnapshotFile Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Read(stream);
    }

    public sealed override ZXSpectrumSnapshotFile Read(Stream stream) => ReadSnapshot(stream);

    [MustUseReturnValue]
    protected abstract ZXSpectrumSnapshotFile ReadSnapshot(Stream stream);
}

public abstract class ZXSpectrumSnapshotFormat<TFile>(string name, string fileExtension) : ZXSpectrumSnapshotFormat(name, fileExtension, typeof(TFile))
    where TFile : ZXSpectrumSnapshotFile
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