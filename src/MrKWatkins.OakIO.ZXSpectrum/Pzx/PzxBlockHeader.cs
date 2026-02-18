namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public abstract class PzxBlockHeader : Header
{
    protected const int StartIndex = 4;

    protected PzxBlockHeader(PzxBlockType type, int sizeIncludingSizeField)
        : base(sizeIncludingSizeField)
    {
        Type = type;
    }

    protected PzxBlockHeader(PzxBlockType type, int sizeIncludingSizeField, Stream stream)
        : base(sizeIncludingSizeField, stream)
    {
        Type = type;
    }

    protected PzxBlockHeader(PzxBlockType type, byte[] data)
        : base(data)
    {
        Type = type;
    }

    public PzxBlockType Type { get; }

    public int SizeOfBlockExcludingTagAndSizeField => (int)GetUInt32(0);

    public int SizeOfHeaderExcludingTagAndSizeField => Data.Count - 4;

    public int BlockLength => SizeOfBlockExcludingTagAndSizeField - SizeOfHeaderExcludingTagAndSizeField;

    public override string ToString() => Type.ToString();
}