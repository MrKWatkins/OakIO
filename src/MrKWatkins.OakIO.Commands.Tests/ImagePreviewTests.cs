using MrKWatkins.OakIO.Commands.FileInfo;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Commands.Tests;

public sealed class ImagePreviewTests
{
    [Test]
    public void Create()
    {
        var image = new ColourImageData(2, 2,
            [new Colour(1, 2, 3, 4), new Colour(5, 6, 7, 8), new Colour(9, 10, 11, 0), new Colour(12, 13, 14)]);
        var preview = ImagePreview.Create(image);
        preview.Width.Should().Equal(2);
        preview.Height.Should().Equal(2);
        preview.Pixels.Should().SequenceEqual(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 0, 12, 13, 14, 255);
    }
}