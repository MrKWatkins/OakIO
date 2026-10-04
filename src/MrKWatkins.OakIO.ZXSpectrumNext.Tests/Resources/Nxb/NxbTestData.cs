using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxb;

internal static class NxbTestData
{
    internal static IEnumerable<TestCaseData> Collections =>
        from bits in new[] { 8, 16 }
        from count in new[] { 0, 1, 2, 3 }
        select new TestCaseData(bits, count);

    internal static byte[] Bytes(int bits, int count)
    {
        byte[] forward = bits == 8 ? [0, 1, 255, 0, 205, 255] : [0, 0, 1, 0, 255, 0, 0, 1, 205, 171, 255, 255];
        byte[] backward = bits == 8 ? [255, 205, 0, 255, 1, 0] : [255, 255, 205, 171, 0, 1, 255, 0, 1, 0, 0, 0];
        return [.. Enumerable.Range(0, count).SelectMany(index => index % 2 == 0 ? forward : backward)];
    }

    internal static TileMapEntry[] Entries(int bits, int count)
    {
        int[] forward = bits == 8 ? [0, 1, 255, 0, 205, 255] : [0, 1, 255, 256, 43981, 65535];
        return [.. Enumerable.Range(0, count).SelectMany(index => index % 2 == 0 ? forward : forward.Reverse()).Select(index => new TileMapEntry(index))];
    }
}