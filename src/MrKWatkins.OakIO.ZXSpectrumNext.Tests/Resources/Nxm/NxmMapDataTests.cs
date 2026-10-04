using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxm;

public sealed class NxmMapDataTests
{
    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Constructor_ByteArray(NxmEncoding encoding, ResourceDataOrder order)
    {
        var format = new NxmFormat(3, 2, encoding, order);
        var bytes = NxmTestData.Bytes(encoding, order);
        var expected = bytes.ToArray();
        var data = new NxmMapData(bytes, format);
        data.Format.Should().BeTheSameInstanceAs(format);
        data.Length.Should().Equal(encoding == NxmEncoding.Index8 ? 6 : 12);
        data.Data.Should().SequenceEqual(expected);
        data.TileMap.Entries.Should().SequenceEqual(NxmTestData.Entries(encoding));
        bytes[^1] ^= 255;
        data.Data.Should().SequenceEqual(expected);
    }

    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Constructor_TileMapData(NxmEncoding encoding, ResourceDataOrder order)
    {
        var data = new NxmMapData(new TileMapData(3, 2, NxmTestData.Entries(encoding)), new NxmFormat(3, 2, encoding, order));
        data.Data.Should().SequenceEqual(NxmTestData.Bytes(encoding, order));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    public void Constructor_ByteArray_InvalidLength(int count) =>
        AssertThat.Invoking(() => new NxmMapData(new byte[count], new NxmFormat(1, 1, NxmEncoding.Tiles512))).Should().Throw<ArgumentException>();

    [TestCase(0x08, 0)]
    [TestCase(0x04, 1)]
    [TestCase(0x02, 2)]
    public void TileMap_IndividualTransform(byte attribute, int flag)
    {
        var entry = new NxmMapData([42, attribute], new NxmFormat(1, 1, NxmEncoding.Tiles512)).TileMap.GetEntry(0, 0);
        entry.TileIndex.Should().Equal(42);
        entry.PaletteOffset.Should().Equal(0);
        entry.MirrorX.Should().Equal(flag == 0);
        entry.MirrorY.Should().Equal(flag == 1);
        entry.Rotate.Should().Equal(flag == 2);
        entry.Priority.Should().BeFalse();
        new NxmFile(new TileMapData(1, 1, [entry]), new NxmFormat(1, 1, NxmEncoding.Tiles512)).ToByteArray().Should().SequenceEqual(42, attribute);
    }
}