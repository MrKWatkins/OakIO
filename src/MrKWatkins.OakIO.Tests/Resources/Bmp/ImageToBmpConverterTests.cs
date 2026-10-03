using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Tests.Resources.Bmp;

public sealed class ImageToBmpConverterTests
{
    [Test]
    public void Constructor()
    {
        var converter = new ImageToBmpConverter(TestImageFormat.Instance);
        converter.SourceFormat.Should().BeTheSameInstanceAs(TestImageFormat.Instance);
        converter.TargetFormat.Should().BeTheSameInstanceAs(BmpFormat.Instance);
    }

    [Test]
    public void Constructor_Null() => AssertThat.Invoking(() => new ImageToBmpConverter(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert()
    {
        var source = new TestImageFile(123);
        var file = new ImageToBmpConverter(source.Format).Convert(source);
        file.Image.Should().BeOfType<ColourImageData>();
        file.Image.GetPixel(0, 0).Should().Equal(new Colour(123, 0, 0));
    }

    [TestCase(1, 8)]
    [TestCase(2, 8)]
    [TestCase(4, 4)]
    [TestCase(8, 8)]
    public void Convert_Indexed(int depth, int expected)
    {
        var palette = new PaletteData([new Colour(1, 2, 3), new Colour(4, 5, 6)]);
        var source = new TestImageFile(new IndexedImageData(2, 1, [1, 0], palette, depth));
        var file = new ImageToBmpConverter(source.Format).Convert(source);
        file.BitsPerPixel.Should().Equal(expected);
        var image = file.Image.Should().BeOfType<IndexedImageData>().Value;
        image.Palette.Colours.Should().SequenceEqual(palette.Colours);
        image.Pixels.Should().SequenceEqual(1, 0);
    }

    [Test]
    public void Convert_Procedural()
    {
        var file = new ImageToBmpConverter(TestImageFormat.Instance).Convert(new TestImageFile(new ProceduralImage()));
        file.Image.Should().BeOfType<ColourImageData>().Value.Pixels.Should().SequenceEqual(
            new Colour(0, 0, 20), new Colour(1, 0, 20), new Colour(2, 0, 20),
            new Colour(0, 1, 20), new Colour(1, 1, 20), new Colour(2, 1, 20));
        AssertThat.Invoking(() => new BmpFile(new ProceduralImage())).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Convert_Null() => AssertThat.Invoking(() => new ImageToBmpConverter(TestImageFormat.Instance).Convert(null!)).Should().Throw<ArgumentNullException>();

    private sealed class ProceduralImage() : ImageData(3, 2)
    {
        public override Colour GetPixel(int x, int y) => new((byte)x, (byte)y, 20);
    }

    [Test]
    public void Convert_Registered()
    {
        var image = new ColourImageData(1, 1, [new Colour(1, 2, 3)]);
        var source = new ConvertibleImageFile(image);
        IOFileConversion.Convert<BmpFile>(source).Image.GetPixel(0, 0).Should().Equal(new Colour(1, 2, 3));
        IOFileConversion.GetSupportedConversionFormats(ConvertibleImageFormat.Instance).Should().SequenceEqual(BmpFormat.Instance);
    }

    [Test]
    public void Convert_Alpha() =>
        AssertThat.Invoking(() => new ImageToBmpConverter(TestImageFormat.Instance).Convert(
            new TestImageFile(new ColourImageData(1, 1, [new Colour(1, 2, 3, 0)]))))
            .Should().Throw<ArgumentException>();

    private sealed class ConvertibleImageFile(ImageData image) : ImageFile(ConvertibleImageFormat.Instance)
    {
        public override ImageData Image { get; } = image;
    }

    private sealed class ConvertibleImageFormat() : ImageFormat<ConvertibleImageFile>("Convertible Image", "convertible")
    {
        public static readonly ConvertibleImageFormat Instance = new();
        protected override IEnumerable<IOFileConverter> CreateConverters() => [new ImageToBmpConverter(this)];
        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => throw new NotSupportedException();
        protected override ValueTask WriteAsync(ConvertibleImageFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}