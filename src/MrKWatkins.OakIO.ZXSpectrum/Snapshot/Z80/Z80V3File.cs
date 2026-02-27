namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// A version 3 Z80 snapshot file.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80V3File : Z80V2OrV3File<Z80V3Header>
{
    internal Z80V3File(Z80V3Header header, [InstantHandle] IEnumerable<Page> pages) : base(header, pages)
    {
    }

    /// <summary>
    /// Creates a new V3 Z80 snapshot file for a 48K Spectrum from the given memory.
    /// </summary>
    /// <param name="memory">The 64K memory contents.</param>
    /// <param name="compress"><c>true</c> to compress the page data; <c>false</c> otherwise.</param>
    /// <returns>A new <see cref="Z80V3File" />.</returns>
    [Pure]
    public static Z80V3File Create48k(Span<byte> memory, bool compress = true) =>
        new(new Z80V3Header(), Page.Create48k(memory, compress));
}