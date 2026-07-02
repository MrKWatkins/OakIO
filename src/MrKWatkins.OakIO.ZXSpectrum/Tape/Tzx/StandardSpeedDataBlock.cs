namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing standard speed data as used by the ROM loading routines.
/// </summary>
public sealed class StandardSpeedDataBlock : TzxBlock<StandardSpeedDataHeader>
{
    internal StandardSpeedDataBlock(byte[] headerData, byte[] bodyData) : base(new StandardSpeedDataHeader(headerData), bodyData)
    {
    }

    /// <summary>
    /// Attempts to interpret this block's data as a standard ZX Spectrum ROM file header.
    /// </summary>
    /// <param name="header">The extracted header, or <c>null</c> if the data is not a valid standard file header.</param>
    /// <returns><c>true</c> if the data was a valid standard file header; <c>false</c> otherwise.</returns>
    [Pure]
    public bool TryGetStandardFileHeader([NotNullWhen(true)] out StandardFileHeader? header) =>
        StandardFileHeader.TryCreate(AsReadOnlySpan(), out header);
}