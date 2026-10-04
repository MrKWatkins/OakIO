using System.Text.Json;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;
using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.Commands.Tests;

[SuppressMessage("Usage", "CA1849:Call async methods when in an async method", Justification = "Tests both synchronous and asynchronous APIs.")]
public sealed class ResourceInfoTests
{
    private static readonly PaletteData Palette = new([new Colour(255, 0, 0), new Colour(0, 255, 0)]);

    [TestCase("gpl")]
    [TestCase("pal")]
    [TestCase("txt")]
    [TestCase("nxp")]
    public async Task GetFileInfo_Palette(string extension)
    {
        PaletteFile file = extension switch
        {
            "gpl" => new GplFile(Palette),
            "pal" => new JascFile(Palette),
            "txt" => new PaintNetFile(Palette),
            _ => new NxpFile(new PaletteData(Enumerable.Repeat(new Colour(255, 0, 0), 16)))
        };
        using var stream = new MemoryStream();
        file.Write(stream);
        var bytes = stream.ToArray();
        var info = InfoCommand.GetFileInfo($"test.{extension}", bytes);
        info.Type.Should().Equal("palette");
        info.Palette!.Should().SequenceEqual(file.Palette.Colours);
        info.Image.Should().BeNull();
        info.Sections[0].Properties[0].Value.Should().Equal(extension == "nxp" ? "16" : "2");
        var asyncInfo = await InfoCommand.GetFileInfoAsync($"test.{extension}", bytes);
        asyncInfo.Palette!.Should().SequenceEqual(info.Palette!);
        using var json = JsonDocument.Parse(await InfoCommand.GetFileInfoJsonAsync($"test.{extension}", bytes));
        json.RootElement.GetProperty("palette")[0].GetProperty("red").GetByte().Should().Equal((byte)255);
        json.RootElement.TryGetProperty("image", out _).Should().BeFalse();
        var output = extension == "pal" ? "output.gpl" : "output.pal";
        var converted = await ConvertCommand.ExecuteAsync($"test.{extension}", bytes, output);
        var convertedPalette = extension == "pal" ? GplFormat.Instance.Read(converted).Palette : JascFormat.Instance.Read(converted).Palette;
        convertedPalette.Colours.Should().SequenceEqual(file.Palette.Colours);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GetFileInfo_Bmp(bool indexed)
    {
        ImageData image = indexed
            ? new IndexedImageData(2, 1, [1, 0], Palette)
            : new ColourImageData(2, 1, [new Colour(0, 255, 0), new Colour(255, 0, 0)]);
        using var stream = new MemoryStream();
        new BmpFile(image).Write(stream);
        var info = InfoCommand.GetFileInfo("test.bmp", stream.ToArray());
        info.Type.Should().Equal("image");
        info.Image!.Width.Should().Equal(2);
        info.Image.Height.Should().Equal(1);
        info.Image.Pixels.Should().SequenceEqual(0, 255, 0, 255, 255, 0, 0, 255);
        (info.Palette is not null).Should().Equal(indexed);
        info.Sections[0].Properties.Select(property => (property.Name, property.Value))
            .Should().SequenceEqual(("Width", "2"), ("Height", "1"));
    }

    [Test]
    public void GetFileInfo_PaintNet_TransparentColours()
    {
        using var stream = new MemoryStream();
        new PaintNetFile(new PaletteData([new Colour(1, 2, 3, 0), new Colour(4, 5, 6, 128)])).Write(stream);
        var info = InfoCommand.GetFileInfo("test.txt", stream.ToArray());
        info.Palette!.Should().SequenceEqual(new Colour(1, 2, 3, 0), new Colour(4, 5, 6, 128));
    }

    [TestCase("scr")]
    [TestCase("nxi")]
    public async Task GetFileInfo_NativeImage(string extension)
    {
        ImageFile file = extension == "scr" ? new ScrFile(new byte[6912]) :
            new NxiFile(new IndexedImageData(256, 192, new byte[256 * 192],
                new PaletteData(Enumerable.Repeat(new Colour(255, 0, 0), 256))));
        using var stream = new MemoryStream();
        file.Write(stream);
        var info = await InfoCommand.GetFileInfoAsync($"test.{extension}", stream.ToArray());
        info.Image!.Width.Should().Equal(256);
        info.Image.Height.Should().Equal(192);
        info.Image.Pixels.Length.Should().Equal(256 * 192 * 4);
        info.Image.Pixels.Take(4).Should().SequenceEqual(extension == "scr" ? new byte[] { 0, 0, 0, 255 } : new byte[] { 255, 0, 0, 255 });
        info.Palette!.Count.Should().Equal(extension == "scr" ? 16 : 256);
        var converted = ConvertCommand.Execute($"test.{extension}", stream.ToArray(), "output.bmp");
        BmpFormat.Instance.Read(converted).Image.GetPixel(0, 0).Should().Equal(file.Image.GetPixel(0, 0));
    }
}