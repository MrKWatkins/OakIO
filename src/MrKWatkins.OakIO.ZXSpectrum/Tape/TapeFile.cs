namespace MrKWatkins.OakIO.ZXSpectrum.Tape;

public abstract class TapeFile : IOFile
{
    private protected TapeFile(TapeFormat format)
        : base(format)
    {
    }

    public new TapeFormat Format => (TapeFormat)base.Format;
}