namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80V3File : Z80V2OrV3File<Z80V3Header>
{
    internal Z80V3File(Z80V3Header header, [InstantHandle] IEnumerable<Page> pages) : base(header, pages)
    {
    }

    [Pure]
    public static Z80V3File Create48k(Span<byte> memory, bool compress = true) =>
        new(new Z80V3Header(), Page.Create48k(memory, compress));
}