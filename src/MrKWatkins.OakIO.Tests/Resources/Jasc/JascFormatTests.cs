using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Jasc;

namespace MrKWatkins.OakIO.Tests.Resources.Jasc;

public sealed class JascFormatTests
{
    internal const string Sample = "\uFEFFJASC-PAL\r\n0100\r\n2\r\n\r\n 1\t2 3\r\n255 128 0\n ";

    [Test]
    public void Read_ByteArray()
    {
        var bytes = Encoding.UTF8.GetBytes(Sample);
        JascFile file = JascFormat.Instance.Read(bytes);
        file.Header.ColourCount.Should().Equal(2);
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(255, 128, 0));
        file.Lines.Count.Should().Equal(4);
        file.Lines[0].Text.Should().Equal("");
        file.Lines[^1].Text.Should().Equal(" ");
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase("")]
    [TestCase("JASC-PAL")]
    [TestCase("RIFF\n0100\n1\n1 2 3")]
    [TestCase("JASC-PAL\n0100\n0")]
    [TestCase("JASC-PAL\n0100\n-1")]
    [TestCase("JASC-PAL\n0100\nfoo")]
    [TestCase("JASC-PAL\n0100\n2147483648")]
    [TestCase("JASC-PAL\n0100\n2\n1 2 3")]
    [TestCase("JASC-PAL\n0100\n1\n1 2 3\n4 5 6")]
    [TestCase("JASC-PAL\n0100\n1\n1 2 3 named")]
    [TestCase("JASC-PAL\n0100\n1\n1 2 256")]
    public void Read_Invalid(string text) => AssertThat.Invoking(() => JascFormat.Instance.Read(Encoding.UTF8.GetBytes(text)))
        .Should().Throw<InvalidDataException>();

    [Test]
    public void Read_UnsupportedVersion() => AssertThat.Invoking(() => JascFormat.Instance.Read("JASC-PAL\n0200\n1\n1 2 3"u8.ToArray()))
        .Should().Throw<NotSupportedException>();

    [Test]
    public async Task ReadAsync_Stream()
    {
        var bytes = Encoding.UTF8.GetBytes(Sample);
        using var stream = new MemoryStream(bytes);
        var file = await JascFormat.Instance.ReadAsync(stream);
        file.Palette.Colours.Should().SequenceEqual(new Colour(1, 2, 3), new Colour(255, 128, 0));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Sample));
        await stream.Awaiting(s => JascFormat.Instance.ReadAsync(s, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void Read_InvalidUtf8() => AssertThat.Invoking(() => JascFormat.Instance.Read(new byte[] { 0xC3, 0x28 }))
        .Should().Throw<InvalidDataException>();
}