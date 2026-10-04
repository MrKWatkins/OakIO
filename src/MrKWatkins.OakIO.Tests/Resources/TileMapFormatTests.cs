using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class TileMapFormatTests
{
    [Test]
    public void Constructor()
    {
        var format = TestTileMapFormat.Instance;
        format.Name.Should().Equal("Test TileMap");
        format.FileExtension.Should().Equal("tilemap");
        format.FileType.Should().Equal(typeof(TestTileMapFile));
    }

    [TestCase(typeof(string))]
    [TestCase(typeof(IOFile))]
    [TestCase(typeof(ResourceFile))]
    [TestCase(typeof(TestResourceFile))]
    [TestCase(typeof(TileMapFile))]
    public void Constructor_InvalidFileType(Type type) =>
        AssertThat.Invoking(() => new InvalidFormat(type)).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Constructor_NullFileType() =>
        AssertThat.Invoking(() => new InvalidFormat(null!)).Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Read_ByteArray()
    {
        TestTileMapFile file = TestTileMapFormat.Instance.Read([123]);
        file.TileMap.Entries[0].TileIndex.Should().Equal(123);
        file.Format.Should().BeTheSameInstanceAs(TestTileMapFormat.Instance);
    }

    [Test]
    public void Read_Stream()
    {
        using var stream = new MemoryStream([234]);
        TestTileMapFile file = TestTileMapFormat.Instance.Read(stream);
        file.TileMap.Entries[0].TileIndex.Should().Equal(234);
    }

    [Test]
    public async Task ReadAsync_Stream()
    {
        using var stream = new MemoryStream([213]);
        TestTileMapFile file = await TestTileMapFormat.Instance.ReadAsync(stream);
        file.TileMap.Entries[0].TileIndex.Should().Equal(213);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream([213]);
        await stream.Awaiting(s => TestTileMapFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void Write()
    {
        var file = new TestTileMapFile(197);
        file.ToByteArray().Should().SequenceEqual(197);
    }

    [Test]
    public async Task WriteAsync()
    {
        using var stream = new MemoryStream();
        await new TestTileMapFile(189).WriteAsync(stream);
        stream.ToArray().Should().SequenceEqual(189);
    }

    [Test]
    public void WriteAsync_WrongFileType()
    {
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        AssertThat.Invoking(() => TestTileMapFormat.Instance.WriteAsync(new TestResourceFile(1), writer))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("file");
    }

    private sealed class InvalidFormat(Type type) : TileMapFormat("Invalid", "invalid", type)
    {
        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => throw new NotSupportedException();
        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}