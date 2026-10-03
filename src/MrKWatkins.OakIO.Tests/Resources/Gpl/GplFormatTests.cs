using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Gpl;

namespace MrKWatkins.OakIO.Tests.Resources.Gpl;

public sealed class GplFormatTests
{
    internal const string Sample = "\uFEFFGIMP Palette\r\nName: Téšt\r\nColumns: 3\r\n# Original\r\n  1\t2  3   名称  \r\n\r\n255 0 128\r\n# tail";

    [Test]
    public void Read_ByteArray()
    {
        var bytes = Encoding.UTF8.GetBytes(Sample);
        GplFile file = GplFormat.Instance.Read(bytes);
        file.Header.Name.Should().Equal("Téšt");
        file.Header.Columns.Should().Equal(3);
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(255, 0, 128));
        file.Entries[0].Name.Should().Equal("名称");
        file.Lines.Count.Should().Equal(5);
        file.Lines[0].Text.Should().Equal("# Original");
        file.Lines[^1].Text.Should().Equal("# tail");
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase("")]
    [TestCase("not GPL\n1 2 3")]
    [TestCase("GIMP Palette\n# empty")]
    [TestCase("GIMP Palette\n")]
    [TestCase("GIMP Palette\nName: Empty\n")]
    [TestCase("GIMP Palette\nName: Test\nColumns: -1\n1 2 3")]
    [TestCase("GIMP Palette\nName: Test\nColumns: 256\n1 2 3")]
    [TestCase("GIMP Palette\n1 2 256")]
    [TestCase("GIMP Palette\nColumns: 2\n1 2 3")]
    public void Read_Invalid(string text) => AssertThat.Invoking(() => GplFormat.Instance.Read(Encoding.UTF8.GetBytes(text)))
        .Should().Throw<InvalidDataException>();

    [Test]
    public async Task ReadAsync_Stream()
    {
        var bytes = Encoding.UTF8.GetBytes(Sample);
        using var stream = new MemoryStream(bytes);
        var file = await GplFormat.Instance.ReadAsync(stream);
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(255, 0, 128));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Sample));
        await stream.Awaiting(s => GplFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void Read_InvalidUtf8() => AssertThat.Invoking(() => GplFormat.Instance.Read(new byte[] { 0xC3, 0x28 }))
        .Should().Throw<InvalidDataException>();
}