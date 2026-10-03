using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources;

public sealed class ResourceConversionsTests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Initialize(int kind)
    {
        // Do not create an NxpFile before requesting the incoming conversion.
        var palette = new PaletteData(Enumerable.Repeat(new Colour(255, 128, 36), 16));
        PaletteFile source = kind switch
        {
            0 => new GplFile(palette),
            1 => new JascFile(palette),
            _ => new PaintNetFile(palette)
        };
        var file = IOFileConversion.Convert<NxpFile>(source);
        file.Entries.Count.Should().Equal(16);
        file.Entries[0].Data.Should().SequenceEqual(0xF0, 1);
        file.Palette.Colours.Should().SequenceEqual(Enumerable.Repeat(new Colour(255, 146, 36), 16));
        IOFileConversion.Convert(source, typeof(NxpFile)).Should().BeOfType<NxpFile>()
            .Value.ToByteArray().Should().SequenceEqual(file.ToByteArray());
        IOFileConversion.Convert(source, NxpFormat.Instance).Should().BeOfType<NxpFile>()
            .Value.ToByteArray().Should().SequenceEqual(file.ToByteArray());
        IOFileConversion.GetSupportedConversionFormats(source.Format).Should().Contain(NxpFormat.Instance);
    }

    [Test]
    public void Initialize_Idempotent()
    {
        ResourceConversions.Initialize();
        ResourceConversions.Initialize();
        var source = new PaintNetFile(new PaletteData(Enumerable.Repeat(new Colour(255, 255, 255), 16)));
        IOFileConversion.Convert<NxpFile>(source).Entries[0].Data.Should().SequenceEqual(255, 1);
        IOFileConversion.GetSupportedConversionFormats(source.Format).Count(format => format == NxpFormat.Instance)
            .Should().Equal(1);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Initialize_Outgoing(int kind)
    {
        var source = NxpFormat.Instance.Read(Nxp.NxpFormatTests.CreateBytes(16));
        PaletteFormat target = kind switch
        {
            0 => GplFormat.Instance,
            1 => JascFormat.Instance,
            _ => PaintNetFormat.Instance
        };
        var result = (PaletteFile)IOFileConversion.Convert(source, target);
        result.Format.Should().BeTheSameInstanceAs(target);
        result.Palette.Colours.Should().SequenceEqual(source.Palette.Colours);
        result.Palette.Colours[1].Should().Equal(new Colour(36, 36, 109));
        IOFileConversion.GetSupportedConversionFormats(source.Format).Should()
            .SequenceEqual(GplFormat.Instance, JascFormat.Instance, PaintNetFormat.Instance);
    }
}