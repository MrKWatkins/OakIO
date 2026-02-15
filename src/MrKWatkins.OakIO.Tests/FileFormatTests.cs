namespace MrKWatkins.OakIO.Tests;

public sealed class FileFormatTests
{
    [Test]
    public void GetFilename() => TestFileFormat.Instance.GetFilename("blah").Should().SequenceEqual("blah.tst");

    [Test]
    public void Read_ByteArray()
    {
        var result = TestFileFormat.Instance.Read(TestFileFormat.Contents);
        result.Should().BeOfType<TestIOFile>().That.Format.Should().BeTheSameInstanceAs(TestFileFormat.Instance);
    }

    [Test]
    public void Write_ThrowsIfWrongFileType()
    {
        var file = new OtherIOFile();
        TestFileFormat.Instance.Invoking(f => f.Write(file)).Should().Throw<ArgumentException>();
    }

    private sealed class OtherIOFile() : IOFile(new OtherFileFormat())
    {
        public override bool TryLoadInto(Span<byte> memory) => throw new NotSupportedException();
    }

    private sealed class OtherFileFormat() : FileFormat("other", "oth")
    {
        public override IOFile Read(Stream stream) => throw new NotSupportedException();

        public override void Write(IOFile file, Stream stream) => throw new NotSupportedException();
    }
}