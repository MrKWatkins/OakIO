namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

public abstract class ZXSpectrumTapeFormat(string name, string fileExtension, Type fileType) : IOFileFormat(name, fileExtension, fileType)
{
    /// <summary>
    /// The T-states per second used for tape loading/saving.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const decimal TStatesPerSecond = 50.08M * 69888;

    [Pure]
    public new ZXSpectrumTapeFile Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Read(stream);
    }

    public sealed override ZXSpectrumTapeFile Read(Stream stream) => ReadTape(stream);

    [MustUseReturnValue]
    protected abstract ZXSpectrumTapeFile ReadTape(Stream stream);
}

public abstract class ZXSpectrumTapeFormat<TFile>(string name, string fileExtension) : ZXSpectrumTapeFormat(name, fileExtension, typeof(TFile))
    where TFile : ZXSpectrumTapeFile
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