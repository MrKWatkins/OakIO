using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources.Scr;

public sealed class ScrFormatTests
{
    [Test]
    public void Instance()
    {
        var format = ScrFormat.Instance;
        format.Name.Should().Equal("ZX Spectrum Screen");
        format.FileExtension.Should().Equal("scr");
        format.FileType.Should().Equal(typeof(ScrFile));
    }
    [Test]
    public void Read_ByteArray()
    {
        var bytes = ScrTestData.Bytes();
        var file = ScrFormat.Instance.Read(bytes);
        file.Render().Pixels.Should().SequenceEqual(ScrTestData.Pixels(bytes, false));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }
    [Test]
    public void Read_Stream()
    {
        var bytes = ScrTestData.Bytes();
        using var stream = new MemoryStream(bytes);
        var file = ScrFormat.Instance.Read(stream);
        file.Render().Pixels.Should().SequenceEqual(ScrTestData.Pixels(bytes, false));
        using var output = new MemoryStream();
        file.Write(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }
    [Test]
    public async Task ReadAsync_Stream()
    {
        var bytes = ScrTestData.Bytes();
        using var stream = new MemoryStream(bytes);
        var file = await ScrFormat.Instance.ReadAsync(stream);
        file.Render().Pixels.Should().SequenceEqual(ScrTestData.Pixels(bytes, false));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }
    [TestCase(0)]
    [TestCase(6911)]
    [TestCase(6913)]
    [TestCase(6976)]
    [TestCase(12288)]
    public void Read_InvalidLength(int length) =>
        AssertThat.Invoking(() => ScrFormat.Instance.Read(new byte[length])).Should().Throw<InvalidDataException>();
    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(new byte[6912]);
        await stream.Awaiting(input => ScrFormat.Instance.ReadAsync(input, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }
    [Test]
    public async Task WriteAsync_Cancelled()
    {
        var file = new ScrFile(new byte[6912]);
        using var stream = new MemoryStream();
        await stream.Awaiting(output => file.WriteAsync(output, cancellationToken: new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }
}