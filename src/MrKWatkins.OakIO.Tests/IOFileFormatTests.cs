using System.IO.Compression;
using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Testing;

namespace MrKWatkins.OakIO.Tests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class IOFileFormatTests
{
    [Test]
    public void Constructor_ThrowsIfFileTypeNotIOFile()
    {
        AssertThat.Invoking(() => new InvalidFileTypeFormat())
            .Should().ThrowArgumentException("Value is not of type IOFile.", "fileType");
    }

    [Test]
    public void FileType() => TestIOFileFormat.Instance.FileType.Should().Equal(typeof(TestIOFile));

    [Test]
    public void CanRead() => TestIOFileFormat.Instance.CanRead.Should().BeTrue();

    [Test]
    public void CanWrite() => TestIOFileFormat.Instance.CanWrite.Should().BeTrue();

    [Test]
    public void GetFilename() => TestIOFileFormat.Instance.GetFilename("blah").Should().SequenceEqual("blah.tst");

    [Test]
    public void Load([Values] bool zipped)
    {
        using var file = TemporaryFile.Create(TestIOFileFormat.Contents, "File.tst", zipped);

        IOFileFormat.Load(file.Path, TestIOFileFormat.Instance).Should().BeOfType<TestIOFile>()
            .That.Format.Should().BeTheSameInstanceAs(TestIOFileFormat.Instance);
    }

    [Test]
    public void Load_Stream()
    {
        using var stream = new MemoryStream(TestIOFileFormat.Contents.ToArray());

        IOFileFormat.Load("File.tst", stream, TestIOFileFormat.Instance).Should().BeOfType<TestIOFile>()
            .That.Format.Should().BeTheSameInstanceAs(TestIOFileFormat.Instance);
    }

    [Test]
    public void Load_ThrowsIfNoSupportedFormats()
    {
        using var stream = new MemoryStream();

        AssertThat.Invoking(() => IOFileFormat.Load("File.tst", stream))
            .Should().ThrowArgumentException("At least one supported format must be provided.", "supportedFormats");
    }

    [Test]
    public void Load_ThrowsIfNoExtension()
    {
        using var stream = new MemoryStream();

        AssertThat.Invoking(() => IOFileFormat.Load("NoExtension", stream, TestIOFileFormat.Instance)).Should().Throw<NotSupportedException>()
            .That.Message.Should().Equal("Files without extensions are not supported.");
    }

    [Test]
    public void Load_ThrowsIfExtensionNotSupported()
    {
        using var stream = new MemoryStream();

        AssertThat.Invoking(() => IOFileFormat.Load("File.xyz", stream, TestIOFileFormat.Instance)).Should().Throw<NotSupportedException>()
            .That.Message.Should().Equal("Files with the extension 'xyz' are not supported.");
    }

    [Test]
    public void Load_ZipSkipsEntriesWithoutASupportedFormat()
    {
        using var zip = CreateZipStream(("NoExtension", [0x09]), ("File.tst", TestIOFileFormat.Contents));

        IOFileFormat.Load("Archive.zip", zip, TestIOFileFormat.Instance).Should().BeOfType<TestIOFile>()
            .That.Format.Should().BeTheSameInstanceAs(TestIOFileFormat.Instance);
    }

    [Test]
    public void Load_ThrowsIfZipHasNoSupportedFormat()
    {
        using var zip = CreateZipStream(("File.xyz", [0x09]));

        AssertThat.Invoking(() => IOFileFormat.Load("Archive.zip", zip, TestIOFileFormat.Instance)).Should().Throw<NotSupportedException>()
            .That.Message.Should().Equal("No file found in ZIP archive of a supported format.");
    }

    [Test]
    public async Task LoadAsync_ThrowsIfZipHasNoSupportedFormat()
    {
        using var zip = CreateZipStream(("File.xyz", [0x09]));

        await zip.Awaiting(z => IOFileFormat.LoadAsync("Archive.zip", z, [TestIOFileFormat.Instance]))
            .Should().ThrowAsync<NotSupportedException>("No file found in ZIP archive of a supported format.");
    }

    [Test]
    public async Task LoadAsync_Stream()
    {
        using var stream = new MemoryStream(TestIOFileFormat.Contents.ToArray());

        var result = await IOFileFormat.LoadAsync("File.tst", stream, [TestIOFileFormat.Instance]);

        result.Should().BeOfType<TestIOFile>().That.Format.Should().BeTheSameInstanceAs(TestIOFileFormat.Instance);
    }

    [Test]
    public async Task ReadAsync()
    {
        using var stream = new MemoryStream(TestIOFileFormat.Contents.ToArray());

        var result = await TestIOFileFormat.Instance.ReadAsync(stream);

        result.Should().BeOfType<TestIOFile>().That.Format.Should().BeTheSameInstanceAs(TestIOFileFormat.Instance);
    }

    [Test]
    public void WriteAsync_ThrowsIfWrongFileType()
    {
        var file = new OtherIOFile();
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        TestIOFileFormat.Instance.Invoking(f => f.WriteAsync(file, writer)).Should().Throw<ArgumentException>();
    }

    [Test]
    public async Task WriteAsync_NonGenericFormatUsesBaseImplementation()
    {
        var format = new NonGenericFormat();
        using var stream = new MemoryStream();
        using (var writer = new SyncStreamBinaryWriter(stream))
        {
            await format.WriteAsync(new TestIOFile(), writer);
        }

        stream.ToArray().Should().SequenceEqual(TestIOFileFormat.Contents);
    }

    [Test]
    public void Read_ByteArray()
    {
        var result = TestIOFileFormat.Instance.Read(TestIOFileFormat.Contents);
        result.Should().BeOfType<TestIOFile>().That.Format.Should().BeTheSameInstanceAs(TestIOFileFormat.Instance);
    }

    [Test]
    public void Write_ThrowsIfWrongFileType()
    {
        var file = new OtherIOFile();
        using var stream = new MemoryStream();
        using var writer = new SyncStreamBinaryWriter(stream);
        TestIOFileFormat.Instance.Invoking(f => f.WriteAsync(file, writer)).Should().Throw<ArgumentException>();
    }

    [MustDisposeResource]
    private static MemoryStream CreateZipStream(params (string Name, byte[] Contents)[] entries)
    {
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, contents) in entries)
            {
                var entry = zip.CreateEntry(name);
                using var entryStream = entry.Open();
                entryStream.Write(contents);
            }
        }

        stream.Position = 0;
        return stream;
    }

    private sealed class OtherIOFile() : IOFile(new OtherIOFileFormat())
    {
        public override bool TryLoadInto(Span<byte> memory) => throw new NotSupportedException();
    }

    // A format deriving directly from the non-generic IOFileFormat, exercising its base WriteAsync implementation.
    private sealed class NonGenericFormat() : IOFileFormat("NonGeneric", "ng", typeof(TestIOFile))
    {
        public override IOFile Read(Stream stream) => throw new NotSupportedException();

        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => writer.WriteAsync(TestIOFileFormat.Contents);
    }

    private sealed class OtherIOFileFormat() : IOFileFormat("other", "oth", typeof(OtherIOFile))
    {
        public override IOFile Read(Stream stream) => throw new NotSupportedException();

        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }

    private sealed class InvalidFileTypeFormat() : IOFileFormat("Invalid", "inv", typeof(string))
    {
        public override IOFile Read(Stream stream) => throw new NotSupportedException();

        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}