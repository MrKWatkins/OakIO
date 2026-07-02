namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing turbo speed data with custom timing parameters.
/// </summary>
public sealed class TurboSpeedDataBlock : TzxBlock<TurboSpeedDataHeader>
{
    internal TurboSpeedDataBlock(byte[] headerData, byte[] data) : base(new TurboSpeedDataHeader(headerData), data)
    {
    }
}