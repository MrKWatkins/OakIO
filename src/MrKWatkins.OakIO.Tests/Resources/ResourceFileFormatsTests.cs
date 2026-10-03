using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ResourceFileFormatsTests
{
    [Test]
    public void AllFormats()
    {
        ResourceFileFormats.AllFormats.Should().SequenceEqual(BmpFormat.Instance);
        using var stream = new MemoryStream(Bmp.BmpFormatTests.Create(24, false));
        var file = IOFileFormat.Load("image.bmp", stream, ResourceFileFormats.AllFormats);
        file.Should().BeOfType<BmpFile>().Value.Image.GetPixel(2, 1).Should().Equal(new Colour(5, 10, 20));
    }
}