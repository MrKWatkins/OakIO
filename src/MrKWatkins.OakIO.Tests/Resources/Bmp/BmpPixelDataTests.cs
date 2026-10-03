using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Tests.Resources.Bmp;

public sealed class BmpPixelDataTests
{
    [TestCase(4, false)]
    [TestCase(4, true)]
    [TestCase(8, false)]
    [TestCase(8, true)]
    [TestCase(24, false)]
    [TestCase(24, true)]
    public void GetImage(int depth, bool topDown)
    {
        var bytes = BmpFormatTests.Create(depth, topDown);
        var file = BmpFormat.Instance.Read(bytes);
        file.PixelData.Data.Should().SequenceEqual(bytes[(depth == 24 ? 54 : 78)..]);
        var image = file.PixelData.GetImage(file.InformationHeader, file.Palette);
        image.GetPixel(0, 0).Should().Equal(new Colour(0, 10, 20));
        image.GetPixel(2, 1).Should().Equal(new Colour(5, 10, 20));
    }

    [Test]
    public void GetImage_NullHeader() => AssertThat.Invoking(() =>
        BmpFormat.Instance.Read(BmpFormatTests.Create(24, false)).PixelData.GetImage(null!, null))
        .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("header");

    [Test]
    public void GetImage_NullPalette()
    {
        var file = BmpFormat.Instance.Read(BmpFormatTests.Create(4, false));
        AssertThat.Invoking(() => file.PixelData.GetImage(file.InformationHeader, null))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("palette");
    }
}