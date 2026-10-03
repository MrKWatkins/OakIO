using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
using MrKWatkins.OakIO.ZXSpectrum.Resources;
using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources;

public sealed class ResourceConversionsTests
{
    [Test]
    public void Initialize()
    {
        ResourceConversions.Initialize();
        ResourceConversions.Initialize();
        var bmp = new BmpFile(new ColourImageData(256, 192, Enumerable.Repeat(new Colour(0, 0, 192), 256 * 192).ToArray()));
        var scr = IOFileConversion.Convert<ScrFile>(bmp);
        scr.Bitmap.Data.Should().SequenceEqual(new byte[6144]);
        scr.Attributes.Data.Should().SequenceEqual(Enumerable.Repeat((byte)9, 768));
        IOFileConversion.Convert<ScrFile>(scr).ToByteArray().Should().SequenceEqual(scr.ToByteArray());
    }
}