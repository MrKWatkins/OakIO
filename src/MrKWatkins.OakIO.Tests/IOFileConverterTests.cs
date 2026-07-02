using MrKWatkins.OakIO.Binary;

namespace MrKWatkins.OakIO.Tests;

public sealed class IOFileConverterTests
{
    [Test]
    public void SourceFormat()
    {
        var converter = new TestToTargetConverter();
        converter.SourceFormat.Should().BeTheSameInstanceAs(TestIOFileFormat.Instance);
    }

    [Test]
    public void TargetFormat()
    {
        var converter = new TestToTargetConverter();
        converter.TargetFormat.Should().BeTheSameInstanceAs(TargetIOFileFormat.Instance);
    }

    [Test]
    public void Convert_IOFile()
    {
        var converter = new TestToTargetConverter();
        var source = new TestIOFile();
        converter.Convert((IOFile)source).Should().BeOfType<TargetIOFile>();
    }

    [Test]
    public void Convert_IOFile_ThrowsIfWrongType()
    {
        var converter = new TestToTargetConverter();
        var wrong = new WrongIOFile();
        converter.Invoking(c => c.Convert(wrong))
            .Should().ThrowArgumentException("Value is not of type TestIOFile.", "source");
    }

    [Test]
    public void Convert_TestIOFile()
    {
        var converter = new TestToTargetConverter();
        var source = new TestIOFile();
        converter.Convert(source).Should().BeOfType<TargetIOFile>();
    }

    private sealed class TargetIOFile() : IOFile(TargetIOFileFormat.Instance);

    private sealed class TargetIOFileFormat() : IOFileFormat<TargetIOFile>("Target", "tgt")
    {
        public static readonly TargetIOFileFormat Instance = new();

        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => new(new TargetIOFile());

        protected override ValueTask WriteAsync(TargetIOFile file, IBinaryWriter writer) => ValueTask.CompletedTask;
    }

    private sealed class TestToTargetConverter() : IOFileConverter<TestIOFile, TargetIOFile>(TestIOFileFormat.Instance, TargetIOFileFormat.Instance)
    {
        public override TargetIOFile Convert(TestIOFile source) => new();
    }

    private sealed class WrongIOFile() : IOFile(WrongIOFileFormat.Instance);

    private sealed class WrongIOFileFormat() : IOFileFormat<WrongIOFile>("Wrong", "wrg")
    {
        public static readonly WrongIOFileFormat Instance = new();

        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => new(new WrongIOFile());

        protected override ValueTask WriteAsync(WrongIOFile file, IBinaryWriter writer) => ValueTask.CompletedTask;
    }
}