using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxi;

public sealed class NxiFormatTests
{
    [Test]
    public void Instance()
    {
        NxiFormat.Instance.Name.Should().Equal("ZX Spectrum Next Image");
        NxiFormat.Instance.FileExtension.Should().Equal("nxi");
        NxiFormat.Instance.FileType.Should().Equal(typeof(NxiFile));
    }

    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public void Read_ByteArray(int width, int height)
    {
        var bytes = NxiTestData.Bytes(width, height);
        var file = NxiFormat.Instance.Read(bytes);
        file.PixelData.Pixels.Should().SequenceEqual(NxiTestData.Pixels(bytes, width, height));
        file.Palette.Palette.Colours.Should().SequenceEqual(NxiTestData.Colours(bytes, width));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public void Read_Stream(int width, int height)
    {
        var bytes = NxiTestData.Bytes(width, height);
        using var stream = new MemoryStream(bytes);
        var file = NxiFormat.Instance.Read(stream);
        file.PixelData.Pixels.Should().SequenceEqual(NxiTestData.Pixels(bytes, width, height));
        using var output = new MemoryStream();
        file.Write(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [TestCase(256, 192)]
    [TestCase(320, 256)]
    [TestCase(640, 256)]
    public async Task ReadAsync_Stream(int width, int height)
    {
        var bytes = NxiTestData.Bytes(width, height);
        using var stream = new MemoryStream(bytes);
        var file = await NxiFormat.Instance.ReadAsync(stream);
        file.PixelData.Pixels.Should().SequenceEqual(NxiTestData.Pixels(bytes, width, height));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [TestCase(0)]
    [TestCase(49663)]
    [TestCase(49665)]
    public void Read_InvalidLength(int length) =>
        AssertThat.Invoking(() => NxiFormat.Instance.Read(new byte[length])).Should().Throw<InvalidDataException>();

    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(new byte[49664]);
        await stream.Awaiting(input => NxiFormat.Instance.ReadAsync(input, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task WriteAsync_Cancelled()
    {
        var file = NxiFormat.Instance.Read(new byte[49664]);
        using var stream = new MemoryStream();
        await stream.Awaiting(output => file.WriteAsync(output, cancellationToken: new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }
}