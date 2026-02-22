namespace MrKWatkins.OakIO.Tests;

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
    public void GetFilename() => TestIOFileFormat.Instance.GetFilename("blah").Should().SequenceEqual("blah.tst");

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
        TestIOFileFormat.Instance.Invoking(f => f.Write(file)).Should().Throw<ArgumentException>();
    }

    private sealed class OtherIOFile() : IOFile(new OtherIOFileFormat())
    {
        public override bool TryLoadInto(Span<byte> memory) => throw new NotSupportedException();
    }

    private sealed class OtherIOFileFormat() : IOFileFormat("other", "oth", typeof(OtherIOFile))
    {
        public override IOFile Read(Stream stream) => throw new NotSupportedException();

        public override void Write(IOFile file, Stream stream) => throw new NotSupportedException();
    }

    private sealed class InvalidFileTypeFormat() : IOFileFormat("Invalid", "inv", typeof(string))
    {
        public override IOFile Read(Stream stream) => throw new NotSupportedException();

        public override void Write(IOFile file, Stream stream) => throw new NotSupportedException();
    }
}