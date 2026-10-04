using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxm;

public sealed class NxmFileTests
{
    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Constructor_ByteArray(NxmEncoding encoding, ResourceDataOrder order)
    {
        var format = new NxmFormat(3, 2, encoding, order);
        var bytes = NxmTestData.Bytes(encoding, order);
        var expected = bytes.ToArray();
        var file = new NxmFile(bytes, format);
        file.Format.Should().BeTheSameInstanceAs(format);
        ((TileMapFile)file).Format.Should().BeTheSameInstanceAs(format);
        file.MapData.Format.Should().BeTheSameInstanceAs(format);
        file.MapData.Data.Should().SequenceEqual(expected);
        file.ToByteArray().Should().SequenceEqual(expected);
        bytes[0] ^= 255;
        file.ToByteArray().Should().SequenceEqual(expected);
    }

    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void TileMap(NxmEncoding encoding, ResourceDataOrder order)
    {
        var format = new NxmFormat(3, 2, encoding, order);
        var bytes = NxmTestData.Bytes(encoding, order);
        var file = format.Read(bytes);
        file.TileMap.Width.Should().Equal(3);
        file.TileMap.Height.Should().Equal(2);
        file.TileMap.Entries.Should().SequenceEqual(NxmTestData.Entries(encoding));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Constructor_TileMapData(NxmEncoding encoding, ResourceDataOrder order)
    {
        var map = new TileMapData(3, 2, NxmTestData.Entries(encoding));
        var file = new NxmFile(map, new NxmFormat(3, 2, encoding, order));
        file.ToByteArray().Should().SequenceEqual(NxmTestData.Bytes(encoding, order));
        file.TileMap.Entries.Should().SequenceEqual(map.Entries);
    }

    [TestCase(0)]
    [TestCase(11)]
    [TestCase(13)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new NxmFile(new byte[length], new NxmFormat(3, 2, NxmEncoding.Tiles512))).Should().Throw<InvalidDataException>();

    [Test]
    public void Constructor_ByteArray_Null() =>
        AssertThat.Invoking(() => new NxmFile(bytes: null!, new NxmFormat(1, 1, NxmEncoding.Index8))).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_TileMapData_Null() =>
        AssertThat.Invoking(() => new NxmFile(map: null!, new NxmFormat(1, 1, NxmEncoding.Index8))).Should().Throw<ArgumentNullException>();

    [Test]
    public void Constructor_NullFormat() =>
        AssertThat.Invoking(() => new NxmFile(new TileMapData(1, 1, [new(0)]), null!)).Should().Throw<ArgumentNullException>();

    [TestCase(2, 3)]
    [TestCase(3, 3)]
    [TestCase(4, 2)]
    public void Constructor_TileMapData_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new NxmFile(new TileMapData(width, height, new TileMapEntry[width * height]), new NxmFormat(3, 2, NxmEncoding.Index8)))
            .Should().Throw<ArgumentException>();

    [TestCase(NxmEncoding.Index8, 256, 0)]
    [TestCase(NxmEncoding.Index8, 257, 0)]
    [TestCase(NxmEncoding.Index16, 65536, 0)]
    [TestCase(NxmEncoding.Index16, 65537, 0)]
    [TestCase(NxmEncoding.Tiles256, 256, 0)]
    [TestCase(NxmEncoding.Tiles512, 512, 0)]
    [TestCase(NxmEncoding.Tiles512, 513, 0)]
    [TestCase(NxmEncoding.Text256, 256, 0)]
    [TestCase(NxmEncoding.Text512, 512, 0)]
    [TestCase(NxmEncoding.Index8, 0, 1)]
    [TestCase(NxmEncoding.Index16, 0, 1)]
    [TestCase(NxmEncoding.Tiles256, 0, 16)]
    [TestCase(NxmEncoding.Tiles512, 0, 16)]
    [TestCase(NxmEncoding.Tiles512, 0, 17)]
    [TestCase(NxmEncoding.Text256, 0, 128)]
    [TestCase(NxmEncoding.Text512, 0, 128)]
    [TestCase(NxmEncoding.Text512, 0, 129)]
    public void Constructor_TileMapData_OutOfRange(NxmEncoding encoding, int index, int palette) =>
        AssertThat.Invoking(() => new NxmFile(new TileMapData(1, 1, [new(index, palette)]), new NxmFormat(1, 1, encoding))).Should().Throw<ArgumentException>();

    [TestCase(NxmEncoding.Index8)]
    [TestCase(NxmEncoding.Index16)]
    [TestCase(NxmEncoding.Text256)]
    [TestCase(NxmEncoding.Text512)]
    public void Constructor_TileMapData_TransformNotSupported(NxmEncoding encoding)
    {
        TileMapEntry[] entries = [new(0) { MirrorX = true }, new(0) { MirrorY = true }, new(0) { Rotate = true }];
        foreach (var entry in entries)
        {
            AssertThat.Invoking(() => new NxmFile(new TileMapData(1, 1, [entry]), new NxmFormat(1, 1, encoding))).Should().Throw<ArgumentException>();
        }
    }

    [TestCase(NxmEncoding.Index8)]
    [TestCase(NxmEncoding.Index16)]
    [TestCase(NxmEncoding.Tiles512)]
    [TestCase(NxmEncoding.Text512)]
    public void Constructor_TileMapData_PriorityNotSupported(NxmEncoding encoding) =>
        AssertThat.Invoking(() => new NxmFile(new TileMapData(1, 1, [new(0) { Priority = true }]), new NxmFormat(1, 1, encoding))).Should().Throw<ArgumentException>();
}