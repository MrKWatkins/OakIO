namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80V2File : Z80V2OrV3File<Z80V2Header>
{
    internal Z80V2File(Z80V2Header header, [InstantHandle] IEnumerable<Page> pages) : base(header, pages)
    {
    }

    [Pure]
    public static Z80V2File Create48k(Span<byte> memory, bool compress = true) =>
        new(new Z80V2Header(), Page.Create48k(memory, compress));
}