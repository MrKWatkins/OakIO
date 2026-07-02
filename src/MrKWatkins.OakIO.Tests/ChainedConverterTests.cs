using MrKWatkins.OakIO.Binary;

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

        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => new(new StubSourceFile());

        protected override ValueTask WriteAsync(StubSourceFile file, IBinaryWriter writer) => ValueTask.CompletedTask;
    }

    private sealed class StubIntermediateFile() : IOFile(StubIntermediateFormat.Instance);

    private sealed class StubIntermediateFormat() : IOFileFormat<StubIntermediateFile>("Intermediate", "int")
    {
        public static readonly StubIntermediateFormat Instance = new();

        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => new(new StubIntermediateFile());

        protected override ValueTask WriteAsync(StubIntermediateFile file, IBinaryWriter writer) => ValueTask.CompletedTask;
    }

    private sealed class StubTargetFile() : IOFile(StubTargetFormat.Instance);

    private sealed class StubTargetFormat() : IOFileFormat<StubTargetFile>("Target", "tgt")
    {
        public static readonly StubTargetFormat Instance = new();

        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => new(new StubTargetFile());

        protected override ValueTask WriteAsync(StubTargetFile file, IBinaryWriter writer) => ValueTask.CompletedTask;
    }

    private sealed class TapToTapeStub() : IOFileConverter<StubSourceFile, StubIntermediateFile>(StubSourceFormat.Instance, StubIntermediateFormat.Instance)
    {
        public override StubIntermediateFile Convert(StubSourceFile source) => new();
    }

    private sealed class TapeToWavStub() : IOFileConverter<StubIntermediateFile, StubTargetFile>(StubIntermediateFormat.Instance, StubTargetFormat.Instance)
    {
        public override StubTargetFile Convert(StubIntermediateFile source) => new();
    }
}