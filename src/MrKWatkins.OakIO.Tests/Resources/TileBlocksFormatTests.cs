using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TileBlocksFormatTests
{
    [Test]
    public void Constructor()
    {
        var format = TestTileBlocksFormat.Instance;
        format.Name.Should().Equal("Test Blocks");
        format.FileExtension.Should().Equal("blocks");
        format.FileType.Should().Equal(typeof(TestTileBlocksFile));
    }

    [TestCase(typeof(string))]
    [TestCase(typeof(IOFile))]
    [TestCase(typeof(ResourceFile))]
    [TestCase(typeof(TestResourceFile))]
    [TestCase(typeof(TileBlocksFile))]
    public void Constructor_InvalidFileType(Type type) =>
        AssertThat.Invoking(() => new InvalidFormat(type)).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Constructor_NullFileType() =>
        AssertThat.Invoking(() => new InvalidFormat(null!)).Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Read_ByteArray()
    {
        TestTileBlocksFile file = TestTileBlocksFormat.Instance.Read([123]);
        file.Blocks.Entries[0].TileIndex.Should().Equal(123);
        file.Format.Should().BeTheSameInstanceAs(TestTileBlocksFormat.Instance);
    }

    [Test]
    public void Read_Stream()
    {
        using var stream = new MemoryStream([234]);
        TestTileBlocksFile file = TestTileBlocksFormat.Instance.Read(stream);
        file.Blocks.Entries[0].TileIndex.Should().Equal(234);
    }

    [Test]
    public async Task ReadAsync_Stream()
    {
        using var stream = new MemoryStream([213]);
        TestTileBlocksFile file = await TestTileBlocksFormat.Instance.ReadAsync(stream);
        file.Blocks.Entries[0].TileIndex.Should().Equal(213);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream([213]);
        await stream.Awaiting(s => TestTileBlocksFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void Write()
    {
        var file = new TestTileBlocksFile(197);
        file.ToByteArray().Should().SequenceEqual(197);
    }

    [Test]
    public async Task WriteAsync()
    {
        using var stream = new MemoryStream();
        await new TestTileBlocksFile(189).WriteAsync(stream);
        stream.ToArray().Should().SequenceEqual(189);
    }

    [Test]
    public void WriteAsync_WrongFileType()
    {
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        AssertThat.Invoking(() => TestTileBlocksFormat.Instance.WriteAsync(new TestResourceFile(1), writer))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("file");
    }

    private sealed class InvalidFormat(Type type) : TileBlocksFormat("Invalid", "invalid", type)
    {
        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => throw new NotSupportedException();
        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}