namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing standard speed data as used by the ROM loading routines.
/// </summary>
public sealed class StandardSpeedDataBlock : TzxBlock<StandardSpeedDataHeader>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StandardSpeedDataBlock"/> class by reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public StandardSpeedDataBlock(Stream stream) : base(new StandardSpeedDataHeader(stream), stream)
    {
    }

    internal StandardSpeedDataBlock(byte[] headerData, byte[] bodyData) : base(new StandardSpeedDataHeader(headerData), bodyData)
    {
    }
}