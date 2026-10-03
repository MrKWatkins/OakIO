using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

internal sealed class TestResourceFile : ResourceFile
{
    public TestResourceFile(byte value) : base(TestResourceFormat.Instance)
    {
        Value = value;
    }

    public TestResourceFile(ResourceFormat format) : base(format)
    {
    }
    public byte Value { get; }
}

internal sealed class TestResourceFormat() : ResourceFormat<TestResourceFile>("Test Resource", "resource")
{
    public static readonly TestResourceFormat Instance = new();

    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new TestResourceFile((await reader.ReadAsync(1))[0]);

    protected override ValueTask WriteAsync(TestResourceFile file, IBinaryWriter writer) => writer.WriteAsync(new[] { file.Value });

    protected override IEnumerable<IOFileConverter> CreateConverters() => [new TestResourceToPaletteConverter()];
}

internal sealed class TestImageFile : ImageFile
{
    public TestImageFile(ImageData image) : this(TestImageFormat.Instance, image)
    {
    }
    public TestImageFile(byte value) : this(new ColourImageData(1, 1, [new Colour(value, 0, 0)]))
    {
    }

    public TestImageFile(ImageFormat format, ImageData image) : base(format)
    {
        ArgumentNullException.ThrowIfNull(image);
        Image = image;
    }

    public override ImageData Image { get; }
}

internal sealed class TestImageFormat() : ImageFormat<TestImageFile>("Test Image", "image")
{
    public static readonly TestImageFormat Instance = new();

    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new TestImageFile((await reader.ReadAsync(1))[0]);

    protected override ValueTask WriteAsync(TestImageFile file, IBinaryWriter writer) => writer.WriteAsync(new[] { file.Image.GetPixel(0, 0).Red });
}

internal sealed class TestPaletteFile : PaletteFile
{
    public TestPaletteFile(PaletteData palette) : this(TestPaletteFormat.Instance, palette)
    {
    }
    public TestPaletteFile(byte value) : this(new PaletteData([new Colour(value, 0, 0)]))
    {
    }

    public TestPaletteFile(PaletteFormat format, PaletteData palette) : base(format)
    {
        ArgumentNullException.ThrowIfNull(palette);
        Palette = palette;
    }

    public override PaletteData Palette { get; }
}

internal sealed class TestPaletteFormat() : PaletteFormat<TestPaletteFile>("Test Palette", "palette")
{
    public static readonly TestPaletteFormat Instance = new();

    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new TestPaletteFile((await reader.ReadAsync(1))[0]);

    protected override ValueTask WriteAsync(TestPaletteFile file, IBinaryWriter writer) => writer.WriteAsync(new[] { file.Palette.Colours[0].Red });
}

internal sealed class TestTilesFile : TilesFile
{
    public TestTilesFile(TilesData tiles) : this(TestTilesFormat.Instance, tiles)
    {
    }
    public TestTilesFile(byte value) : this(new TilesData(1, 1, [value]))
    {
    }

    public TestTilesFile(TilesFormat format, TilesData tiles) : base(format)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        Tiles = tiles;
    }

    public override TilesData Tiles { get; }
}

internal sealed class TestTilesFormat() : TilesFormat<TestTilesFile>("Test Tiles", "tiles")
{
    public static readonly TestTilesFormat Instance = new();

    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new TestTilesFile((await reader.ReadAsync(1))[0]);

    protected override ValueTask WriteAsync(TestTilesFile file, IBinaryWriter writer) => writer.WriteAsync(new[] { file.Tiles.Pixels[0] });
}

internal sealed class TestResourceToPaletteConverter() : IOFileConverter<TestResourceFile, TestPaletteFile>(TestResourceFormat.Instance, TestPaletteFormat.Instance)
{
    public override TestPaletteFile Convert(TestResourceFile source) => new(source.Value);
}