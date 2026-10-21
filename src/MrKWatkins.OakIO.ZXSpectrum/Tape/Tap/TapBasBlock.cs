namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Describes a code block to load with TapBAS. Bank selection is loader behaviour, not TAP header metadata.
/// </summary>
/// <param name="Location">The CPU address to load the bytes to.</param>
/// <param name="Data">The bytes to load. File creation copies the data.</param>
public sealed record TapBasBlock(ushort Location, ReadOnlyMemory<byte> Data)
{
    /// <summary>
    /// The physical 128K RAM bank, from 0 to 7, to select before loading at $C000–$FFFF.
    /// Null loads without changing the current mapping.
    /// </summary>
    public byte? Bank { get; init; }
}