using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.Commands.Tests.FileInfo;

public sealed class TapInfoExtensionsTests
{
    [Test]
    public void ToInfoSections()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var sections = tap.ToInfoSections();
        sections.Should().HaveCount(1);
        sections[0].Title.Should().Equal("Blocks");
        sections[0].Items.Count.Should().Equal(2);
    }

    [Test]
    public void ToInfoSections_HeaderBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var sections = tap.ToInfoSections();
        var item = sections[0].Items[0];
        item.Title.Should().Equal("Bytes: test");
        var typeProperty = item.Properties.Single(p => p.Name == "Type");
        typeProperty.Value.Should().Equal("Header");
        var headerTypeProperty = item.Properties.Single(p => p.Name == "Header Type");
        headerTypeProperty.Value.Should().Equal("Code");
        var filenameProperty = item.Properties.Single(p => p.Name == "Filename");
        filenameProperty.Value.Should().Equal("test");
    }

    [Test]
    public void ToInfoSections_DataBlock()
    {
        var tap = TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]);
        var sections = tap.ToInfoSections();
        var item = sections[0].Items[1];
        item.Title.Should().Equal("Data: 2 bytes");
        var typeProperty = item.Properties.Single(p => p.Name == "Type");
        typeProperty.Value.Should().Equal("Data");
        var lengthProperty = item.Properties.Single(p => p.Name == "Length");
        lengthProperty.Value.Should().Equal("2");
    }
}