using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ImageFileTests
{
    [Test]
    public void Constructor()
    {
        var data = new ColourImageData(1, 1, [new Colour(1, 2, 3)]);
        var file = new TestImageFile(data);
        file.Image.Should().BeTheSameInstanceAs(data);
        file.Format.Should().BeTheSameInstanceAs(TestImageFormat.Instance);
        ((ResourceFile)file).Format.Should().BeTheSameInstanceAs(TestImageFormat.Instance);
        ((IOFile)file).Format.Should().BeTheSameInstanceAs(TestImageFormat.Instance);
    }

    [Test]
    public void Constructor_NullFormat() =>
        AssertThat.Invoking(() => new TestImageFile(null!, new ColourImageData(1, 1, [default])))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("format");

    [Test]
    public void Constructor_NullData() =>
        AssertThat.Invoking(() => new TestImageFile(TestImageFormat.Instance, null!))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("image");
}