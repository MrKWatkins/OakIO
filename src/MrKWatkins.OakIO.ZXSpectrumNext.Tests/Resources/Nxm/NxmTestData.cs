using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxm;

internal static class NxmTestData
{
    internal static IEnumerable<TestCaseData> Layouts =>
        from encoding in Enum.GetValues<NxmEncoding>()
        from order in Enum.GetValues<ResourceDataOrder>()
        select new TestCaseData(encoding, order);

    internal static byte[] Bytes(NxmEncoding encoding, ResourceDataOrder order)
    {
        int[] values = [0, 1, 255, 256, 0xABCD, 65535];
        int[] permutation = order == ResourceDataOrder.RowMajor ? [0, 1, 2, 3, 4, 5] : [0, 3, 1, 4, 2, 5];
        return [.. permutation.SelectMany(index => encoding == NxmEncoding.Index8
            ? new[] { (byte)values[index] }
            : new[] { (byte)values[index], (byte)(values[index] / 256) })];
    }

    // Independent expectations use literal attribute meanings, not the production decoder.
    internal static TileMapEntry[] Entries(NxmEncoding encoding) => encoding switch
    {
        NxmEncoding.Index8 => [new(0), new(1), new(255), new(0), new(205), new(255)],
        NxmEncoding.Index16 => [new(0), new(1), new(255), new(256), new(43981), new(65535)],
        NxmEncoding.Tiles256 => [new(0), new(1), new(255), new(0) { Priority = true },
            new(205, 10) { MirrorX = true, Rotate = true, Priority = true },
            new(255, 15) { MirrorX = true, MirrorY = true, Rotate = true, Priority = true }],
        NxmEncoding.Tiles512 => [new(0), new(1), new(255), new(256),
            new(461, 10) { MirrorX = true, Rotate = true },
            new(511, 15) { MirrorX = true, MirrorY = true, Rotate = true }],
        NxmEncoding.Text256 => [new(0), new(1), new(255), new(0) { Priority = true },
            new(205, 85) { Priority = true }, new(255, 127) { Priority = true }],
        _ => [new(0), new(1), new(255), new(256), new(461, 85), new(511, 127)]
    };
}