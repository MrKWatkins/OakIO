using MrKWatkins.OakIO.Compression;
using MrKWatkins.OakIO.Testing;

namespace MrKWatkins.OakIO.Tests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class IOFileTests
{
    [TestCase(CompressionFormat.None, "{name}.tst")]
    [TestCase(CompressionFormat.Zip, "{name}.zip")]
    public void Save_Load_Roundtrip(CompressionFormat compressionFormat, string expectedFilenameFormat)
    {
        using var temporaryDirectory = TemporaryDirectory.Create();
        var ioFile = new TestIOFile();

        var name = Guid.NewGuid().ToString();
        var actualPath = ioFile.Save(temporaryDirectory.Path, name, compressionFormat);
        var expectedPath = Path.Combine(temporaryDirectory.Path, expectedFilenameFormat.Replace("{name}", name, StringComparison.OrdinalIgnoreCase));
        actualPath.Should().Equal(expectedPath);

        var roundTripped = IOFileFormat.Load(expectedPath, TestIOFileFormat.Instance);

        roundTripped.ToByteArray().Should().SequenceEqual(ioFile.ToByteArray());
    }

    [TestCase(CompressionFormat.None, "{name}.tst")]
    [TestCase(CompressionFormat.Zip, "{name}.zip")]
    public async Task SaveAsync_LoadAsync_Roundtrip(CompressionFormat compressionFormat, string expectedFilenameFormat)
    {
        using var temporaryDirectory = TemporaryDirectory.Create();
        var ioFile = new TestIOFile();

        var name = Guid.NewGuid().ToString();
        var actualPath = await ioFile.SaveAsync(temporaryDirectory.Path, name, compressionFormat);
        var expectedPath = Path.Combine(temporaryDirectory.Path, expectedFilenameFormat.Replace("{name}", name, StringComparison.OrdinalIgnoreCase));
        actualPath.Should().Equal(expectedPath);

        var roundTripped = await IOFileFormat.LoadAsync(expectedPath, [TestIOFileFormat.Instance]);

        roundTripped.ToByteArray().Should().SequenceEqual(ioFile.ToByteArray());
    }

    [Test]
    public void Save_ThrowsIfDirectoryDoesNotExist()
    {
        var ioFile = new TestIOFile();
        var missingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        ioFile.Invoking(f => f.Save(missingDirectory, "File")).Should().Throw<DirectoryNotFoundException>()
            .That.Message.Should().Equal($"The output directory '{missingDirectory}' does not exist.");
    }

    [Test]
    public void Save_ThrowsIfCompressionFormatNotSupported()
    {
        using var temporaryDirectory = TemporaryDirectory.Create();
        var ioFile = new TestIOFile();
        const CompressionFormat unsupported = (CompressionFormat)byte.MaxValue;

        ioFile.Invoking(f => f.Save(temporaryDirectory.Path, "File", unsupported)).Should().Throw<NotSupportedException>()
            .That.Message.Should().Equal($"Compression format {unsupported} is not supported.");
    }

    [Test]
    public void Write_Stream()
    {
        var ioFile = new TestIOFile();
        using var actual = TemporaryFile.Create();
        using (var stream = actual.OpenWrite())
        {
            ioFile.Write(stream);
        }

        using var expected = TemporaryFile.Create(TestIOFileFormat.Contents);
        actual.Bytes.Should().SequenceEqual(expected.Bytes);
    }

    [Test]
    public async Task WriteAsync_Stream()
    {
        var ioFile = new TestIOFile();
        using var actual = TemporaryFile.Create();
        await using (var stream = actual.OpenWrite())
        {
            await ioFile.WriteAsync(stream);
        }

        using var expected = TemporaryFile.Create(TestIOFileFormat.Contents);
        actual.Bytes.Should().SequenceEqual(expected.Bytes);
    }

    [Test]
    public void Write_ByteArray()
    {
        var ioFile = new TestIOFile();
        var actual = ioFile.ToByteArray();
        actual.Should().SequenceEqual(TestIOFileFormat.Contents);
    }

    [Test]
    public void TryLoadInto_CanLoad()
    {
        IOFile ioFile = new TestIOFile();
        var buffer = new byte[5];
        ioFile.TryLoadInto(buffer).Should().BeTrue();
        buffer.Should().SequenceEqual(TestIOFileFormat.Contents);
    }

    [Test]
    public void TryLoadInto_CannotLoad()
    {
        IOFile ioFile = new TestIOFile(false);
        var buffer = new byte[5];
        ioFile.TryLoadInto(buffer).Should().BeFalse();
    }

    [Test]
    public void LoadInto_CanLoad()
    {
        IOFile ioFile = new TestIOFile();
        var buffer = new byte[5];
        ioFile.LoadInto(buffer);
        buffer.Should().SequenceEqual(TestIOFileFormat.Contents);
    }

    [Test]
    public void LoadInto_CannotLoad()
    {
        IOFile ioFile = new TestIOFile(false);
        var buffer = new byte[5];
        ioFile.Invoking(f => f.LoadInto(buffer)).Should().Throw<IOException>();
    }

    [Test]
    public void TryLoadInto_DefaultReturnsFalse()
    {
        IOFile ioFile = new NonLoadableIOFile();
        var buffer = new byte[5];
        ioFile.TryLoadInto(buffer).Should().BeFalse();
    }

    [Test]
    public void LoadInto_DefaultThrows()
    {
        IOFile ioFile = new NonLoadableIOFile();
        var buffer = new byte[5];
        ioFile.Invoking(f => f.LoadInto(buffer)).Should().Throw<IOException>();
    }

    private sealed class NonLoadableIOFile() : IOFile(TestIOFileFormat.Instance);
}