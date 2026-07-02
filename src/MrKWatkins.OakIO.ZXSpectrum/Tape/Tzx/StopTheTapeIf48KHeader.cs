namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX stop the tape if 48K block.
/// </summary>
public sealed class StopTheTapeIf48KHeader : TzxBlockHeader
{
    internal StopTheTapeIf48KHeader(byte[] data)
        : base(TzxBlockType.StopTheTapeIf48K, data)
    {
    }
}