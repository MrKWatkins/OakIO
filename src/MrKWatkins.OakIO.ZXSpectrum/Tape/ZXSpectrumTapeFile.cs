namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

public abstract class ZXSpectrumTapeFile : IOFile
{
    private protected ZXSpectrumTapeFile(ZXSpectrumTapeFormat format)
        : base(format)
    {
    }

    public new ZXSpectrumTapeFormat Format => (ZXSpectrumTapeFormat)base.Format;
}