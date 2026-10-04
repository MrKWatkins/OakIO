using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxm;

public sealed class TileMapToNxmConverterTests
{
    [Test]
    public void Constructor()
    {
        var source = new NxmFormat(1, 1, NxmEncoding.Index8);
        var target = new NxmFormat(1, 1, NxmEncoding.Index16);
        var converter = new TileMapToNxmConverter(source, target);
        converter.SourceFormat.Should().BeTheSameInstanceAs(source);
        converter.TargetFormat.Should().BeTheSameInstanceAs(target);
    }

    [Test]
    public void Constructor_NullSource() =>
        AssertThat.Invoking(() => new TileMapToNxmConverter(null!, new NxmFormat(1, 1, NxmEncoding.Index8))).Should().Throw<ArgumentNullException>();
    [Test]
    public void Constructor_NullTarget() =>
        AssertThat.Invoking(() => new TileMapToNxmConverter(new NxmFormat(1, 1, NxmEncoding.Index8), null!)).Should().Throw<ArgumentNullException>();

    [TestCaseSource(typeof(NxmTestData), nameof(NxmTestData.Layouts))]
    public void Convert_TileMapFile(NxmEncoding encoding, ResourceDataOrder order)
    {
        var source = new NxmFormat(3, 2, encoding).Read(NxmTestData.Bytes(encoding, ResourceDataOrder.RowMajor));
        var target = new NxmFormat(3, 2, encoding, order);
        var file = new TileMapToNxmConverter(source.Format, target).Convert(source);
        file.Should().NotBeTheSameInstanceAs(source);
        file.MapData.Should().NotBeTheSameInstanceAs(source.MapData);
        file.Format.Should().BeTheSameInstanceAs(target);
        file.ToByteArray().Should().SequenceEqual(NxmTestData.Bytes(encoding, order));
    }

    [TestCase(NxmEncoding.Index8, NxmEncoding.Index16)]
    [TestCase(NxmEncoding.Index16, NxmEncoding.Index8)]
    public void Convert_TileMapFile_IndexWidth(NxmEncoding sourceEncoding, NxmEncoding targetEncoding)
    {
        var sourceFormat = new NxmFormat(2, 1, sourceEncoding);
        var source = new NxmFile(new TileMapData(2, 1, [new(0), new(255)]), sourceFormat);
        var target = new NxmFormat(2, 1, targetEncoding);
        var file = new TileMapToNxmConverter(sourceFormat, target).Convert(source);
        file.ToByteArray().Should().SequenceEqual(targetEncoding == NxmEncoding.Index8 ? new byte[] { 0, 255 } : new byte[] { 0, 0, 255, 0 });
    }

    [Test]
    public void Convert_TileMapFile_IndexOverflow()
    {
        var source = new NxmFile(new TileMapData(1, 1, [new(256)]), new NxmFormat(1, 1, NxmEncoding.Index16));
        AssertThat.Invoking(() => new TileMapToNxmConverter(source.Format, new NxmFormat(1, 1, NxmEncoding.Index8)).Convert(source)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Convert_TileMapFile_AttributeLoss()
    {
        var source = new NxmFormat(1, 1, NxmEncoding.Tiles512).Read([42, 8]);
        AssertThat.Invoking(() => new TileMapToNxmConverter(source.Format, new NxmFormat(1, 1, NxmEncoding.Index16)).Convert(source)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Convert_TileMapFile_Null() =>
        AssertThat.Invoking(() => new TileMapToNxmConverter(new NxmFormat(1, 1, NxmEncoding.Index8), new NxmFormat(1, 1, NxmEncoding.Index8)).Convert(null!))
            .Should().Throw<ArgumentNullException>();

    [Test]
    public void Convert_IOFile()
    {
        var format = new NxmFormat(1, 1, NxmEncoding.Index8);
        IOFileConverter converter = new TileMapToNxmConverter(format, format);
        converter.Convert(format.Read([42])).ToByteArray().Should().SequenceEqual(42);
    }

    [Test]
    public void Convert_TileMapFile_PriorityLoss()
    {
        var source = new NxmFormat(1, 1, NxmEncoding.Tiles256).Read([42, 1]);
        AssertThat.Invoking(() => new TileMapToNxmConverter(source.Format, new NxmFormat(1, 1, NxmEncoding.Tiles512)).Convert(source))
            .Should().Throw<ArgumentException>();
        source.ToByteArray().Should().SequenceEqual(42, 1);
    }

    [Test]
    public void Convert_TileMapFile_PaletteLoss()
    {
        var source = new NxmFormat(1, 1, NxmEncoding.Tiles512).Read([42, 16]);
        AssertThat.Invoking(() => new TileMapToNxmConverter(source.Format, new NxmFormat(1, 1, NxmEncoding.Index8)).Convert(source))
            .Should().Throw<ArgumentException>();
        source.ToByteArray().Should().SequenceEqual(42, 16);
    }

    [Test]
    public void Convert_TileMapFile_InvalidDimensions()
    {
        var source = new NxmFormat(2, 1, NxmEncoding.Index8).Read([42, 43]);
        AssertThat.Invoking(() => new TileMapToNxmConverter(source.Format, new NxmFormat(1, 2, NxmEncoding.Index8)).Convert(source))
            .Should().Throw<ArgumentException>();
    }
}