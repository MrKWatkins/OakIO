namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

// TODO: Move to IO. Move sound stuff too. Add ToWav.
public abstract class TapeFormat(string name, string fileExtension) : FileFormat(name, fileExtension)
{
    [Pure]
    public new TapeFile Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Read(stream);
    }

    public sealed override TapeFile Read(Stream stream) => ReadTape(stream);

    [MustUseReturnValue]
    protected abstract TapeFile ReadTape(Stream stream);
}

public abstract class TapeFormat<TFile>(string name, string fileExtension) : TapeFormat(name, fileExtension)
    where TFile : TapeFile
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