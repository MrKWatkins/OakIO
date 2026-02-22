namespace MrKWatkins.OakIO.Tests;

public sealed class ChainedConverterTests
{
    [Test]
    public void SourceFormat()
    {
        var tapToTape = new TapToTapeStub();
        var tapeToWav = new TapeToWavStub();
        var converter = new ChainedConverter<StubSourceFile, StubIntermediateFile, StubTargetFile>(tapToTape, tapeToWav);

        converter.SourceFormat.Should().BeTheSameInstanceAs(StubSourceFormat.Instance);
    }

    [Test]
    public void TargetFormat()
    {
        var tapToTape = new TapToTapeStub();
        var tapeToWav = new TapeToWavStub();
        var converter = new ChainedConverter<StubSourceFile, StubIntermediateFile, StubTargetFile>(tapToTape, tapeToWav);

        converter.TargetFormat.Should().BeTheSameInstanceAs(StubTargetFormat.Instance);
    }

    [Test]
    public void Convert()
    {
        var tapToTape = new TapToTapeStub();
        var tapeToWav = new TapeToWavStub();
        var converter = new ChainedConverter<StubSourceFile, StubIntermediateFile, StubTargetFile>(tapToTape, tapeToWav);
        var source = new StubSourceFile();

        var result = converter.Convert(source);

        result.Should().BeOfType<StubTargetFile>();
    }

    private sealed class StubSourceFile() : IOFile(StubSourceFormat.Instance);

    private sealed class StubSourceFormat() : IOFileFormat<StubSourceFile>("Source", "src")
    {
        public static readonly StubSourceFormat Instance = new();

        public override IOFile Read(Stream stream) => new StubSourceFile();

        protected override void Write(StubSourceFile file, Stream stream) { }
    }

    private sealed class StubIntermediateFile() : IOFile(StubIntermediateFormat.Instance);

    private sealed class StubIntermediateFormat() : IOFileFormat<StubIntermediateFile>("Intermediate", "int")
    {
        public static readonly StubIntermediateFormat Instance = new();

        public override IOFile Read(Stream stream) => new StubIntermediateFile();

        protected override void Write(StubIntermediateFile file, Stream stream) { }
    }

    private sealed class StubTargetFile() : IOFile(StubTargetFormat.Instance);

    private sealed class StubTargetFormat() : IOFileFormat<StubTargetFile>("Target", "tgt")
    {
        public static readonly StubTargetFormat Instance = new();

        public override IOFile Read(Stream stream) => new StubTargetFile();

        protected override void Write(StubTargetFile file, Stream stream) { }
    }

    private sealed class TapToTapeStub() : IOFileConverter<StubSourceFile, StubIntermediateFile>(StubSourceFormat.Instance, StubIntermediateFormat.Instance)
    {
        public override StubIntermediateFile Convert(StubSourceFile source) => new StubIntermediateFile();
    }

    private sealed class TapeToWavStub() : IOFileConverter<StubIntermediateFile, StubTargetFile>(StubIntermediateFormat.Instance, StubTargetFormat.Instance)
    {
        public override StubTargetFile Convert(StubIntermediateFile source) => new StubTargetFile();
    }
}