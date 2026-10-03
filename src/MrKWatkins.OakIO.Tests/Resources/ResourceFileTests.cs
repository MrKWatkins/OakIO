namespace MrKWatkins.OakIO.Tests.Resources;

public sealed class ResourceFileTests
{
    [Test]
    public void Constructor()
    {
        var file = new TestResourceFile(1);
        file.Value.Should().Equal((byte)1);
        file.Format.Should().BeTheSameInstanceAs(TestResourceFormat.Instance);
        file.Format.Should().BeTheSameInstanceAs(TestResourceFormat.Instance);
        ((IOFile)file).Format.Should().BeTheSameInstanceAs(TestResourceFormat.Instance);
    }

    [Test]
    public void Constructor_NullFormat() =>
        AssertThat.Invoking(() => new TestResourceFile(null!))
            .Should().Throw<ArgumentNullException>().That.ParamName.Should().Equal("format");
}