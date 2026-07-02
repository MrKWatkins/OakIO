namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing pure data with no pilot tone.
/// </summary>
public sealed class PureDataBlock : TzxBlock<PureDataHeader>
{
    internal PureDataBlock(byte[] headerData, byte[] data) : base(new PureDataHeader(headerData), data)
    {
    }
}