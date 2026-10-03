using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.Tests.Resources.PaintNet;

public sealed class PaintNetFormatTests
{
    internal const string Sample = "\uFEFF; Original 名\r\n\r\n  80010203  \r\n; middle\nffFF8000\r\n; tail";

    [Test]
    public void Read_ByteArray()
    {
        var bytes = Encoding.UTF8.GetBytes(Sample);
        PaintNetFile file = PaintNetFormat.Instance.Read(bytes);
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3, 128), new Colour(255, 128, 0));
        file.Lines.Count.Should().Equal(6);
        file.Lines[0].Text.Should().Equal("; Original 名");
        file.Lines[^1].Text.Should().Equal("; tail");
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase("")]
    [TestCase("; comment only")]
    [TestCase("   \n")]
    [TestCase("FFFFFF")]
    [TestCase("FFFFFFFFF")]
    [TestCase("GG010203")]
    [TestCase("FF010203 ; inline comment")]
    [TestCase("JASC-PAL\n0100\n1\n1 2 3")]
    public void Read_Invalid(string text) => AssertThat.Invoking(() => PaintNetFormat.Instance.Read(Encoding.UTF8.GetBytes(text)))
        .Should().Throw<InvalidDataException>();

    [Test]
    public async Task ReadAsync_Stream()
    {
        var bytes = Encoding.UTF8.GetBytes(Sample);
        using var stream = new MemoryStream(bytes);
        var file = await PaintNetFormat.Instance.ReadAsync(stream);
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3, 128), new Colour(255, 128, 0));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Sample));
        await stream.Awaiting(s => PaintNetFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void Read_InvalidUtf8() => AssertThat.Invoking(() => PaintNetFormat.Instance.Read([0xC3, 0x28]))
        .Should().Throw<InvalidDataException>();
}