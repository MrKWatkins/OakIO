using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxp;

public sealed class NxpFormatTests
{
    [Test]
    public void Instance()
    {
        NxpFormat.Instance.Name.Should().Equal("ZX Spectrum Next Palette");
        NxpFormat.Instance.FileExtension.Should().Equal("nxp");
        NxpFormat.Instance.FileType.Should().Equal(typeof(NxpFile));
    }

    [TestCase(16)]
    [TestCase(256)]
    public void Read_ByteArray(int count)
    {
        var bytes = CreateBytes(count);
        var file = NxpFormat.Instance.Read(bytes);
        file.Entries.Count.Should().Equal(count);
        file.Palette.Colours[0].Should().Equal(new Colour(0, 0, 0));
        file.Palette.Colours[1].Should().Equal(new Colour(36, 36, 109));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase(16)]
    [TestCase(256)]
    public void Read_Stream(int count)
    {
        var bytes = CreateBytes(count);
        using var stream = new MemoryStream(bytes);
        var file = NxpFormat.Instance.Read(stream);
        file.Palette.Colours[1].Should().Equal(new Colour(36, 36, 109));
        using var output = new MemoryStream();
        file.Write(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [TestCase(16)]
    [TestCase(256)]
    public async Task ReadAsync_Stream(int count)
    {
        var bytes = CreateBytes(count);
        using var stream = new MemoryStream(bytes);
        var file = await NxpFormat.Instance.ReadAsync(stream);
        file.Entries.Count.Should().Equal(count);
        file.Palette.Colours[1].Should().Equal(new Colour(36, 36, 109));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(new byte[32]);
        await stream.Awaiting(input => NxpFormat.Instance.ReadAsync(input, new CancellationToken(true)))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task WriteAsync_Cancelled()
    {
        var file = NxpFormat.Instance.Read(new byte[32]);
        using var stream = new MemoryStream();
        await stream.Awaiting(output => file.WriteAsync(output, cancellationToken: new CancellationToken(true)))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(31)]
    [TestCase(33)]
    [TestCase(256)]
    [TestCase(511)]
    [TestCase(513)]
    public void Read_InvalidLength(int length) =>
        AssertThat.Invoking(() => NxpFormat.Instance.Read(new byte[length])).Should().Throw<InvalidDataException>();

    [Test]
    public void Read_InvalidEntry()
    {
        var bytes = new byte[512];
        bytes[^1] = 128;
        AssertThat.Invoking(() => NxpFormat.Instance.Read(bytes)).Should().Throw<InvalidDataException>();
    }

    [Pure]
    internal static byte[] CreateBytes(int count) =>
        [.. Enumerable.Range(0, count).SelectMany(index => new[] { (byte)(index * 37), (byte)(index & 1) })];
}