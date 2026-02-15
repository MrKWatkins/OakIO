using MrKWatkins.OakIO.Testing;

namespace MrKWatkins.OakIO.Tests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class IOFileTests
{
    [Test]
    public void Read_File([Values] bool zipped)
    {
        using var file = TemporaryFile.Create(TestFileFormat.Contents, "File.tst", zipped);

        IOFile.Read(file.Path, TestFileFormat.Instance).Should().BeOfType<TestIOFile>()
            .That.Format.Should().BeTheSameInstanceAs(TestFileFormat.Instance);
    }

    [Test]
    public void Read_File_ThrowsForUnsupportedType([Values] bool zipped)
    {
        using var file = TemporaryFile.Create(TestFileFormat.Contents, "File.txt", zipped);

        AssertThat.Invoking(() => IOFile.Read(file.Path, TestFileFormat.Instance)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Read_Stream([Values] bool zipped)
    {
        using var file = TemporaryFile.Create(TestFileFormat.Contents, "File.tst", zipped);

        using var stream = file.OpenRead();
        IOFile.Read(file.Name, stream, TestFileFormat.Instance).Should().BeOfType<TestIOFile>()
            .That.Format.Should().BeTheSameInstanceAs(TestFileFormat.Instance);
    }

    [Test]
    public void Read_Stream_ThrowsForUnsupportedType([Values] bool zipped)
    {
        using var file = TemporaryFile.Create(TestFileFormat.Contents, "File.txt", zipped);

        using var stream = file.OpenRead();
        AssertThat.Invoking(() => IOFile.Read(file.Path, stream, TestFileFormat.Instance)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Write_Path_InvalidExtension([Values] bool zipped)
    {
        var ioFile = new TestIOFile();
        var directory = Path.GetTempPath();
        var path = Path.Combine(directory, $"{Guid.NewGuid().ToString()}.invalid");
        ioFile.Invoking(i => i.Write(path, zipped)).Should().ThrowArgumentException("Value has the extension .invalid rather than the expected .tst.", "filePath");
    }

    [Test]
    public void Write_Path([Values] bool zipped)
    {
        var ioFile = new TestIOFile();
        var directory = Path.GetTempPath();
        var name = $"{Guid.NewGuid().ToString()}.tst";
        var expectedPath = Path.Combine(directory, zipped ? $"{name}.zip" : name);

        try
        {
            ioFile.Write(Path.Combine(directory, name), zipped);
            var actual = File.ReadAllBytes(expectedPath);

            using var expected = TemporaryFile.Create(TestFileFormat.Contents, name, zipped: zipped);
            actual.Should().SequenceEqual(expected.Bytes);
        }
        finally
        {
            File.Delete(expectedPath);
        }
    }

    [Test]
    public void Write_Directory_Name([Values] bool zipped)
    {
        var ioFile = new TestIOFile();
        var directory = Path.GetTempPath();
        var name = Guid.NewGuid().ToString();
        var expectedPath = Path.Combine(directory, zipped ? $"{name}.tst.zip" : $"{name}.tst");

        try
        {
            ioFile.Write(directory, name, zipped);
            var actual = File.ReadAllBytes(expectedPath);

            using var expected = TemporaryFile.Create(TestFileFormat.Contents, $"{name}.tst", zipped: zipped);
            actual.Should().SequenceEqual(expected.Bytes);
        }
        finally
        {
            File.Delete(expectedPath);
        }
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

        using var expected = TemporaryFile.Create(TestFileFormat.Contents);
        actual.Bytes.Should().SequenceEqual(expected.Bytes);
    }

    [Test]
    public void Write_ByteArray()
    {
        var ioFile = new TestIOFile();
        var actual = ioFile.Write();
        actual.Should().SequenceEqual(TestFileFormat.Contents);
    }

    [Test]
    public void TryLoadInto_CanLoad()
    {
        IOFile ioFile = new TestIOFile();
        var buffer = new byte[5];
        ioFile.TryLoadInto(buffer).Should().BeTrue();
        buffer.Should().SequenceEqual(TestFileFormat.Contents);
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
        buffer.Should().SequenceEqual(TestFileFormat.Contents);
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

    private sealed class NonLoadableIOFile() : IOFile(TestFileFormat.Instance);
}