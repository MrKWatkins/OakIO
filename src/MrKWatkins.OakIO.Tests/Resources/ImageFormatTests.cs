using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ImageFormatTests
{
    [Test]
    public void Constructor()
    {
        var format = TestImageFormat.Instance;
        format.Name.Should().Equal("Test Image");
        format.FileExtension.Should().Equal("image");
        format.FileType.Should().Equal(typeof(TestImageFile));
    }

    [TestCase(typeof(string))]
    [TestCase(typeof(IOFile))]
    [TestCase(typeof(ResourceFile))]
    [TestCase(typeof(TestResourceFile))]
    [TestCase(typeof(ImageFile))]
    public void Constructor_InvalidFileType(Type type) =>
        AssertThat.Invoking(() => new InvalidFormat(type)).Should().Throw<ArgumentException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Constructor_NullFileType() =>
        AssertThat.Invoking(() => new InvalidFormat(null!)).Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("fileType");

    [Test]
    public void Read_ByteArray()
    {
        TestImageFile file = TestImageFormat.Instance.Read(new byte[] { 123 });
        file.Image.GetPixel(0, 0).Red.Should().Equal((byte)123);
        file.Format.Should().BeTheSameInstanceAs(TestImageFormat.Instance);
    }

    [Test]
    public void Read_Stream()
    {
        using var stream = new MemoryStream(new byte[] { 234 });
        TestImageFile file = TestImageFormat.Instance.Read(stream);
        file.Image.GetPixel(0, 0).Red.Should().Equal((byte)234);
    }

    [Test]
    public async Task ReadAsync_Stream()
    {
        using var stream = new MemoryStream(new byte[] { 213 });
        TestImageFile file = await TestImageFormat.Instance.ReadAsync(stream);
        file.Image.GetPixel(0, 0).Red.Should().Equal((byte)213);
    }

    [Test]
    public async Task ReadAsync_Stream_Cancelled()
    {
        using var stream = new MemoryStream(new byte[] { 213 });
        await stream.Awaiting(s => TestImageFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void Write()
    {
        var file = new TestImageFile(197);
        file.ToByteArray().Should().SequenceEqual(197);
    }

    [Test]
    public async Task WriteAsync()
    {
        using var stream = new MemoryStream();
        await new TestImageFile(189).WriteAsync(stream);
        stream.ToArray().Should().SequenceEqual(189);
    }

    [Test]
    public void WriteAsync_WrongFileType()
    {
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        AssertThat.Invoking(() => TestImageFormat.Instance.WriteAsync(new TestResourceFile(1), writer))
            .Should().Throw<ArgumentException>().That.ParamName.Should().Equal("file");
    }

    private sealed class InvalidFormat(Type type) : ImageFormat("Invalid", "invalid", type)
    {
        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => throw new NotSupportedException();
        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}