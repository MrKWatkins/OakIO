using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class PaletteFormatTests
{
    [Test]
    public void Constructor()
    {
        var format = TestPaletteFormat.Instance;
        format.Name.Should().Equal("Test Palette");
        format.FileExtension.Should().Equal("palette");
        format.FileType.Should().Equal(typeof(TestPaletteFile));
    }

    [TestCase(typeof(string))]
    [TestCase(typeof(IOFile))]
    [TestCase(typeof(ResourceFile))]
    [TestCase(typeof(TestResourceFile))]
    [TestCase(typeof(PaletteFile))]
    public void Constructor_InvalidFileType(Type type) =>
        AssertThat.Invoking(() => new InvalidFormat(type)).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Constructor_NullFileType() =>
        AssertThat.Invoking(() => new InvalidFormat(null!)).Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Read_ByteArray()
    {
        TestPaletteFile file = TestPaletteFormat.Instance.Read([123]);
        file.Palette.Colours[0].Red.Should().Equal((byte)123);
        file.Format.Should().BeTheSameInstanceAs(TestPaletteFormat.Instance);
    }

    [Test]
    public void Read_Stream()
    {
        using var stream = new MemoryStream([234]);
        TestPaletteFile file = TestPaletteFormat.Instance.Read(stream);
        file.Palette.Colours[0].Red.Should().Equal((byte)234);
    }

    [Test]
    public async Task ReadAsync_Stream()
    {
        using var stream = new MemoryStream([213]);
        TestPaletteFile file = await TestPaletteFormat.Instance.ReadAsync(stream);
        file.Palette.Colours[0].Red.Should().Equal((byte)213);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream([213]);
        await stream.Awaiting(s => TestPaletteFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void Write()
    {
        var file = new TestPaletteFile(197);
        file.ToByteArray().Should().SequenceEqual(197);
    }

    [Test]
    public async Task WriteAsync()
    {
        using var stream = new MemoryStream();
        await new TestPaletteFile(189).WriteAsync(stream);
        stream.ToArray().Should().SequenceEqual(189);
    }

    [Test]
    public void WriteAsync_WrongFileType()
    {
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        AssertThat.Invoking(() => TestPaletteFormat.Instance.WriteAsync(new TestResourceFile(1), writer))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("file");
    }

    private sealed class InvalidFormat(Type type) : PaletteFormat("Invalid", "invalid", type)
    {
        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => throw new NotSupportedException();
        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}