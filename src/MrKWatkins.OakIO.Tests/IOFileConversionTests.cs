namespace MrKWatkins.OakIO.Tests;

public sealed class IOFileConversionTests
{
    [Test]
    public void Convert_Generic()
    {
        var source = new ConversionSourceFile();
        IOFileConversion.Convert<ConversionTargetFile>(source).Should().BeOfType<ConversionTargetFile>();
    }

    [Test]
    public void Convert_IOFileFormat()
    {
        var source = new ConversionSourceFile();
        IOFileConversion.Convert(source, ConversionTargetFileFormat.Instance).Should().BeOfType<ConversionTargetFile>();
    }

    [Test]
    public void Convert_IOFileFormat_ThrowsIfNoConverter()
    {
        var source = new ConversionSourceFile();
        AssertThat.Invoking(() => IOFileConversion.Convert(source, UnregisteredFileFormat.Instance))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Convert_Type()
    {
        var source = new ConversionSourceFile();
        IOFileConversion.Convert(source, typeof(ConversionTargetFile)).Should().BeOfType<ConversionTargetFile>();
    }

    [Test]
    public void Convert_Type_ThrowsIfNotIOFile()
    {
        var source = new ConversionSourceFile();
        AssertThat.Invoking(() => IOFileConversion.Convert(source, typeof(string)))
            .Should().ThrowArgumentException("Value must be an IOFile.", "targetType");
    }

    [Test]
    public void Convert_Type_ThrowsIfNoConverter()
    {
        var source = new ConversionSourceFile();
        AssertThat.Invoking(() => IOFileConversion.Convert(source, typeof(UnregisteredFile)))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void GetSupportedConversionFormats()
    {
        _ = new ConversionSourceFile();
        IOFileConversion.GetSupportedConversionFormats(ConversionSourceFileFormat.Instance)
            .Should().SequenceEqual(ConversionSecondTargetFileFormat.Instance, ConversionTargetFileFormat.Instance);
    }

    [Test]
    public void RegisterConverters_SameTypeIgnored()
    {
        _ = new ConversionSourceFile();
        IOFileConversion.RegisterConverters(new SourceToTargetConverter());
    }

    [Test]
    public void RegisterConverters_ThrowsIfDifferentTypeAlreadyRegistered()
    {
        _ = new ConversionSourceFile();
        AssertThat.Invoking(() => IOFileConversion.RegisterConverters(new AlternativeSourceToTargetConverter()))
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void TryConvert_Success()
    {
        var source = new ConversionSourceFile();

        IOFileConversion.TryConvert(source, ConversionTargetFileFormat.Instance, out var result, out var error).Should().BeTrue();

        result.Should().BeOfType<ConversionTargetFile>();
        error.Should().BeNull();
    }

    [Test]
    public void TryConvert_NoConverterRegistered()
    {
        var source = new ConversionSourceFile();

        IOFileConversion.TryConvert(source, UnregisteredFileFormat.Instance, out var result, out var error).Should().BeFalse();

        result.Should().BeNull();
        error.Should().NotBeNull();
    }

    [Test]
    public void TryConvert_ConverterThrows()
    {
        var source = new ThrowingSourceFile();

        IOFileConversion.TryConvert(source, ThrowingTargetFileFormat.Instance, out var result, out var error).Should().BeFalse();

        result.Should().BeNull();
        error.Should().Equal("Conversion failed.");
    }

    private sealed class ConversionSourceFile() : IOFile(ConversionSourceFileFormat.Instance);

    private sealed class ConversionSourceFileFormat() : IOFileFormat<ConversionSourceFile>("Source", "src")
    {
        public static readonly ConversionSourceFileFormat Instance = new();

        protected override IEnumerable<IOFileConverter> CreateConverters() =>
            [new SourceToTargetConverter(), new SourceToSecondTargetConverter()];

        public override IOFile Read(Stream stream) => new ConversionSourceFile();

        protected override void Write(ConversionSourceFile file, Stream stream) { }
    }

    private sealed class ConversionTargetFile() : IOFile(ConversionTargetFileFormat.Instance);

    private sealed class ConversionTargetFileFormat() : IOFileFormat<ConversionTargetFile>("Target", "tgt2")
    {
        public static readonly ConversionTargetFileFormat Instance = new();

        public override IOFile Read(Stream stream) => new ConversionTargetFile();

        protected override void Write(ConversionTargetFile file, Stream stream) { }
    }

    private sealed class ConversionSecondTargetFile() : IOFile(ConversionSecondTargetFileFormat.Instance);

    private sealed class ConversionSecondTargetFileFormat() : IOFileFormat<ConversionSecondTargetFile>("Another Target", "tgt3")
    {
        public static readonly ConversionSecondTargetFileFormat Instance = new();

        public override IOFile Read(Stream stream) => new ConversionSecondTargetFile();

        protected override void Write(ConversionSecondTargetFile file, Stream stream) { }
    }

    private sealed class SourceToTargetConverter() : IOFileConverter<ConversionSourceFile, ConversionTargetFile>(ConversionSourceFileFormat.Instance, ConversionTargetFileFormat.Instance)
    {
        public override ConversionTargetFile Convert(ConversionSourceFile source) => new ConversionTargetFile();
    }

    private sealed class SourceToSecondTargetConverter() : IOFileConverter<ConversionSourceFile, ConversionSecondTargetFile>(ConversionSourceFileFormat.Instance, ConversionSecondTargetFileFormat.Instance)
    {
        public override ConversionSecondTargetFile Convert(ConversionSourceFile source) => new ConversionSecondTargetFile();
    }

    private sealed class AlternativeSourceToTargetConverter() : IOFileConverter<ConversionSourceFile, ConversionTargetFile>(ConversionSourceFileFormat.Instance, ConversionTargetFileFormat.Instance)
    {
        public override ConversionTargetFile Convert(ConversionSourceFile source) => new ConversionTargetFile();
    }

    private sealed class UnregisteredFile() : IOFile(UnregisteredFileFormat.Instance);

    private sealed class UnregisteredFileFormat() : IOFileFormat<UnregisteredFile>("Unregistered", "unr")
    {
        public static readonly UnregisteredFileFormat Instance = new();

        public override IOFile Read(Stream stream) => new UnregisteredFile();

        protected override void Write(UnregisteredFile file, Stream stream) { }
    }

    private sealed class ThrowingSourceFile() : IOFile(ThrowingSourceFileFormat.Instance);

    private sealed class ThrowingSourceFileFormat() : IOFileFormat<ThrowingSourceFile>("ThrowingSource", "ths")
    {
        public static readonly ThrowingSourceFileFormat Instance = new();

        protected override IEnumerable<IOFileConverter> CreateConverters() =>
            [new ThrowingConverter()];

        public override IOFile Read(Stream stream) => new ThrowingSourceFile();

        protected override void Write(ThrowingSourceFile file, Stream stream) { }
    }

    private sealed class ThrowingTargetFile() : IOFile(ThrowingTargetFileFormat.Instance);

    private sealed class ThrowingTargetFileFormat() : IOFileFormat<ThrowingTargetFile>("ThrowingTarget", "tht")
    {
        public static readonly ThrowingTargetFileFormat Instance = new();

        public override IOFile Read(Stream stream) => new ThrowingTargetFile();

        protected override void Write(ThrowingTargetFile file, Stream stream) { }
    }

    private sealed class ThrowingConverter() : IOFileConverter<ThrowingSourceFile, ThrowingTargetFile>(ThrowingSourceFileFormat.Instance, ThrowingTargetFileFormat.Instance)
    {
        public override ThrowingTargetFile Convert(ThrowingSourceFile source) => throw new NotSupportedException("Conversion failed.");
    }
}