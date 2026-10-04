using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Compression;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxb;

public sealed class NxbFormatTests
{
    [TestCase(8)]
    [TestCase(16)]
    public void Constructor(int bits)
    {
        var format = new NxbFormat(3, 2, bits);
        format.Name.Should().Equal("ZX Spectrum Next Blocks");
        format.FileExtension.Should().Equal("nxb");
        format.FileType.Should().Equal(typeof(NxbFile));
        format.Width.Should().Equal(3);
        format.Height.Should().Equal(2);
        format.BitsPerIndex.Should().Equal(bits);
        format.BytesPerBlock.Should().Equal(bits == 8 ? 6 : 12);
    }

    [TestCase(0, 1)]
    [TestCase(-1, 1)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
    public void Constructor_InvalidDimensions(int width, int height) =>
        AssertThat.Invoking(() => new NxbFormat(width, height, 8)).Should().Throw<ArgumentOutOfRangeException>();
    [TestCase(0)]
    [TestCase(4)]
    [TestCase(9)]
    [TestCase(17)]
    public void Constructor_InvalidBitsPerIndex(int bits) =>
        AssertThat.Invoking(() => new NxbFormat(1, 1, bits)).Should().Throw<ArgumentOutOfRangeException>().That.ParamName.Should().Equal("bitsPerIndex");
    [TestCase(int.MaxValue, 2, 8)]
    [TestCase(int.MaxValue, 1, 16)]
    public void Constructor_Overflow(int width, int height, int bits) =>
        AssertThat.Invoking(() => new NxbFormat(width, height, bits)).Should().Throw<OverflowException>();

    [TestCaseSource(typeof(NxbTestData), nameof(NxbTestData.Collections))]
    public void Read_ByteArray(int bits, int count)
    {
        var bytes = NxbTestData.Bytes(bits, count);
        var file = new NxbFormat(3, 2, bits).Read(bytes);
        file.Blocks.Entries.Should().SequenceEqual(NxbTestData.Entries(bits, count));
        file.Entries.Count.Should().Equal(count);
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCaseSource(typeof(NxbTestData), nameof(NxbTestData.Collections))]
    public void Read_Stream(int bits, int count)
    {
        var bytes = NxbTestData.Bytes(bits, count);
        using var stream = new MemoryStream(bytes);
        var file = new NxbFormat(3, 2, bits).Read(stream);
        file.Blocks.Entries.Should().SequenceEqual(NxbTestData.Entries(bits, count));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCaseSource(typeof(NxbTestData), nameof(NxbTestData.Collections))]
    public async Task ReadAsync_Stream(int bits, int count)
    {
        var bytes = NxbTestData.Bytes(bits, count);
        using var stream = new MemoryStream(bytes);
        var file = await new NxbFormat(3, 2, bits).ReadAsync(stream);
        file.Blocks.Entries.Should().SequenceEqual(NxbTestData.Entries(bits, count));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream([42]);
        await stream.Awaiting(s => new NxbFormat(1, 1, 8).ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }
    [Test]
    public async Task WriteAsync_Stream_Cancelled()
    {
        var file = new NxbFormat(1, 1, 8).Read([42]);
        using var stream = new MemoryStream();
        await file.Awaiting(f => f.WriteAsync(stream, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [TestCase(2, 1, 8)]
    [TestCase(1, 2, 8)]
    [TestCase(1, 1, 16)]
    public async Task WriteAsync_InvalidLayout(int width, int height, int bits)
    {
        TileBlocksFormat<NxbFile> target = new NxbFormat(1, 1, 8);
        var source = new NxbFormat(width, height, bits).Read([]);
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        await target.Awaiting(t => t.WriteAsync(source, writer).AsTask()).Should().ThrowAsync<ArgumentException>();
        stream.Length.Should().Equal(0L);
    }

    [Test]
    public async Task WriteAsync_EquivalentLayout()
    {
        TileBlocksFormat<NxbFile> target = new NxbFormat(1, 1, 8);
        var source = new NxbFormat(1, 1, 8).Read([42]);
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        await target.WriteAsync(source, writer);
        stream.ToArray().Should().SequenceEqual(42);
    }

    [TestCaseSource(typeof(NxbTestData), nameof(NxbTestData.Collections))]
    public void Read_Discovery(int bits, int count)
    {
        var format = new NxbFormat(3, 2, bits);
        ResourceFormat[] formats = [.. ZXSpectrumNextResourceFileFormats.AllFormats, format];
        var bytes = NxbTestData.Bytes(bits, count);
        using var input = new MemoryStream(bytes);
        ((NxbFile)IOFileFormat.Load("blocks.nxb", input, formats)).ToByteArray().Should().SequenceEqual(bytes);
        using var compressed = new MemoryStream();
        format.Read(bytes).Write(compressed, "blocks.nxb", CompressionFormat.GZip);
        compressed.Position = 0;
        var file = (NxbFile)IOFileFormat.Load("blocks.nxb.gz", compressed, formats);
        file.Format.Should().BeTheSameInstanceAs(format);
        file.Blocks.Entries.Should().SequenceEqual(NxbTestData.Entries(bits, count));
        file.ToByteArray().Should().SequenceEqual(bytes);
        ZXSpectrumNextResourceFileFormats.AllFormats.Select(f => f.FileExtension).Should().NotContain("nxb");
    }

    [Test]
    public async Task ReadAsync_Discovery()
    {
        var format = new NxbFormat(3, 2, 16);
        using var input = new MemoryStream(NxbTestData.Bytes(16, 2));
        var file = (NxbFile)await IOFileFormat.LoadAsync("blocks.nxb", input, [.. ZXSpectrumNextResourceFileFormats.AllFormats, format]);
        file.Blocks.Entries.Should().SequenceEqual(NxbTestData.Entries(16, 2));
        file.ToByteArray().Should().SequenceEqual(NxbTestData.Bytes(16, 2));
    }
}