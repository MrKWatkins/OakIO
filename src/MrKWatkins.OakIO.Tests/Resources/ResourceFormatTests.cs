using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ResourceFormatTests
{
    [Test]
    public void Constructor()
    {
        var format = TestResourceFormat.Instance;
        format.Name.Should().Equal("Test Resource");
        format.FileExtension.Should().Equal("resource");
        format.FileType.Should().Equal(typeof(TestResourceFile));
    }

    [TestCase(typeof(string))]
    [TestCase(typeof(IOFile))]
    [TestCase(typeof(ResourceFile))]
    public void Constructor_InvalidFileType(Type type) =>
        AssertThat.Invoking(() => new InvalidFormat(type)).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Constructor_NullFileType() =>
        AssertThat.Invoking(() => new InvalidFormat(null!)).Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Read_ByteArray()
    {
        TestResourceFile file = TestResourceFormat.Instance.Read([123]);
        file.Value.Should().Equal((byte)123);
        file.Format.Should().BeTheSameInstanceAs(TestResourceFormat.Instance);
    }

    [Test]
    public void Read_Stream()
    {
        using var stream = new MemoryStream([234]);
        TestResourceFile file = TestResourceFormat.Instance.Read(stream);
        file.Value.Should().Equal((byte)234);
    }

    [Test]
    public async Task ReadAsync_Stream()
    {
        using var stream = new MemoryStream([213]);
        TestResourceFile file = await TestResourceFormat.Instance.ReadAsync(stream);
        file.Value.Should().Equal((byte)213);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream([213]);
        await stream.Awaiting(s => TestResourceFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void Write()
    {
        var file = new TestResourceFile(197);
        file.ToByteArray().Should().SequenceEqual(197);
    }

    [Test]
    public async Task WriteAsync()
    {
        using var stream = new MemoryStream();
        await new TestResourceFile(189).WriteAsync(stream);
        stream.ToArray().Should().SequenceEqual(189);
    }

    [Test]
    public void WriteAsync_WrongFileType()
    {
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        AssertThat.Invoking(() => TestResourceFormat.Instance.WriteAsync(new TestPaletteFile(1), writer))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("file");
    }

    [Test]
    public void CreateConverters()
    {
        var source = new TestResourceFile(123);
        var converted = IOFileConversion.Convert<TestPaletteFile>(source);
        converted.Palette.Colours.Should().SequenceEqual(new Colour(123, 0, 0));
        IOFileConversion.GetSupportedConversionFormats(TestResourceFormat.Instance).Should().SequenceEqual(TestPaletteFormat.Instance);
    }

    private sealed class InvalidFormat(Type type) : ResourceFormat("Invalid", "invalid", type)
    {
        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => throw new NotSupportedException();
        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}