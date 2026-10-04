using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Compression;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxm;

public sealed class NxmFormatTests
{
    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Constructor(NxmEncoding encoding, ResourceDataOrder order)
    {
        var format = new NxmFormat(3, 2, encoding, order);
        format.Name.Should().Equal("ZX Spectrum Next Map");
        format.FileExtension.Should().Equal("nxm");
        format.FileType.Should().Equal(typeof(NxmFile));
        format.Width.Should().Equal(3);
        format.Height.Should().Equal(2);
        format.Encoding.Should().Equal(encoding);
        format.Order.Should().Equal(order);
        format.BytesPerEntry.Should().Equal(encoding == NxmEncoding.Index8 ? 1 : 2);
        format.Length.Should().Equal(encoding == NxmEncoding.Index8 ? 6 : 12);
    }

    [TestCase(0, 1)]
    [TestCase(-1, 1)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
    public void Constructor_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new NxmFormat(width, height, NxmEncoding.Index8)).Should().Throw<ArgumentOutOfRangeException>();

    [TestCase(-1)]
    [TestCase(6)]
    public void Constructor_InvalidEncoding(int encoding) =>
        AssertThat.Invoking(() => new NxmFormat(1, 1, (NxmEncoding)encoding)).Should().Throw<ArgumentOutOfRangeException>();

    [TestCase(-1)]
    [TestCase(2)]
    public void Constructor_InvalidOrder(int order) =>
        AssertThat.Invoking(() => new NxmFormat(1, 1, NxmEncoding.Index8, (ResourceDataOrder)order)).Should().Throw<ArgumentOutOfRangeException>();

    [TestCase(int.MaxValue, 2, NxmEncoding.Index8)]
    [TestCase(int.MaxValue, 1, NxmEncoding.Index16)]
    public void Constructor_Overflow(int width, int height, NxmEncoding encoding) =>
        AssertThat.Invoking(() => new NxmFormat(width, height, encoding)).Should().Throw<OverflowException>();

    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Read_ByteArray(NxmEncoding encoding, ResourceDataOrder order)
    {
        var bytes = NxmTestData.Bytes(encoding, order);
        var file = new NxmFormat(3, 2, encoding, order).Read(bytes);
        file.TileMap.Entries.Should().SequenceEqual(NxmTestData.Entries(encoding));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Read_Stream(NxmEncoding encoding, ResourceDataOrder order)
    {
        var bytes = NxmTestData.Bytes(encoding, order);
        using var stream = new MemoryStream(bytes);
        var file = new NxmFormat(3, 2, encoding, order).Read(stream);
        file.TileMap.Entries.Should().SequenceEqual(NxmTestData.Entries(encoding));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public async Task ReadAsync_Stream(NxmEncoding encoding, ResourceDataOrder order)
    {
        var bytes = NxmTestData.Bytes(encoding, order);
        using var stream = new MemoryStream(bytes);
        var file = await new NxmFormat(3, 2, encoding, order).ReadAsync(stream);
        file.TileMap.Entries.Should().SequenceEqual(NxmTestData.Entries(encoding));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream([42]);
        await stream.Awaiting(s => new NxmFormat(1, 1, NxmEncoding.Index8).ReadAsync(s, new CancellationToken(true)))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task WriteAsync_Stream_Cancelled()
    {
        var file = new NxmFormat(1, 1, NxmEncoding.Index8).Read([42]);
        using var stream = new MemoryStream();
        await file.Awaiting(f => f.WriteAsync(stream, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [TestCase(2, 1, NxmEncoding.Index8, ResourceDataOrder.RowMajor)]
    [TestCase(1, 2, NxmEncoding.Index8, ResourceDataOrder.RowMajor)]
    [TestCase(1, 1, NxmEncoding.Index16, ResourceDataOrder.RowMajor)]
    [TestCase(1, 1, NxmEncoding.Index8, ResourceDataOrder.ColumnMajor)]
    public void WriteAsync_InvalidLayout(int width, int height, NxmEncoding encoding, ResourceDataOrder order)
    {
        TileMapFormat<NxmFile> target = new NxmFormat(1, 1, NxmEncoding.Index8);
        var source = new NxmFormat(width, height, encoding, order).Read(new byte[width * height * (encoding == NxmEncoding.Index8 ? 1 : 2)]);
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        AssertThat.Invoking(() => target.WriteAsync(source, writer)).Should().Throw<ArgumentException>();
        stream.Length.Should().Equal(0L);
    }

    [Test]
    public async Task WriteAsync_EquivalentLayout()
    {
        TileMapFormat<NxmFile> target = new NxmFormat(1, 1, NxmEncoding.Index8);
        var source = new NxmFormat(1, 1, NxmEncoding.Index8).Read([42]);
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        await target.WriteAsync(source, writer);
        stream.ToArray().Should().SequenceEqual(42);
    }

    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Read_Discovery(NxmEncoding encoding, ResourceDataOrder order)
    {
        var format = new NxmFormat(3, 2, encoding, order);
        ResourceFormat[] formats = [.. ZXSpectrumNextResourceFileFormats.AllFormats, format];
        var bytes = NxmTestData.Bytes(encoding, order);
        using var input = new MemoryStream(bytes);
        ((NxmFile)IOFileFormat.Load("map.nxm", input, formats)).ToByteArray().Should().SequenceEqual(bytes);
        using var compressed = new MemoryStream();
        format.Read(bytes).Write(compressed, "map.nxm", CompressionFormat.GZip);
        compressed.Position = 0;
        var file = (NxmFile)IOFileFormat.Load("map.nxm.gz", compressed, formats);
        file.Format.Should().BeTheSameInstanceAs(format);
        file.TileMap.Entries.Should().SequenceEqual(NxmTestData.Entries(encoding));
        file.ToByteArray().Should().SequenceEqual(bytes);
        ZXSpectrumNextResourceFileFormats.AllFormats.Select(f => f.FileExtension).Should().NotContain("nxm");
    }
}