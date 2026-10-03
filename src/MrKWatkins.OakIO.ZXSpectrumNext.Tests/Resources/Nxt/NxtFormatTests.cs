using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxt;

public sealed class NxtFormatTests
{
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void ForBitsPerPixel(int bits)
    {
        var format = NxtFormat.ForBitsPerPixel(bits);
        format.BitsPerPixel.Should().Equal(bits);
        format.Name.Should().Equal($"ZX Spectrum Next Tiles ({bits}-bit)");
        format.FileExtension.Should().Equal("nxt");
        format.FileType.Should().Equal(typeof(NxtFile));
        NxtFormat.ForBitsPerPixel(bits).Should().BeTheSameInstanceAs(format);
    }
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(2)]
    [TestCase(16)]
    public void ForBitsPerPixel_Invalid(int bits) =>
        AssertThat.Invoking(() => NxtFormat.ForBitsPerPixel(bits)).Should().Throw<ArgumentOutOfRangeException>();
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Read_ByteArray(int bits)
    {
        var bytes = TileTestData.Bytes(8, bits, 2);
        var file = NxtFormat.ForBitsPerPixel(bits).Read(bytes);
        file.Entries.Count.Should().Equal(2);
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Read_Stream(int bits)
    {
        var bytes = TileTestData.Bytes(8, bits, 2);
        using var stream = new MemoryStream(bytes);
        var file = NxtFormat.ForBitsPerPixel(bits).Read(stream);
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
        using var output = new MemoryStream();
        file.Write(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public async Task ReadAsync_Stream(int bits)
    {
        var bytes = TileTestData.Bytes(8, bits, 2);
        using var stream = new MemoryStream(bytes);
        var file = await NxtFormat.ForBitsPerPixel(bits).ReadAsync(stream);
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Read_Empty(int bits)
    {
        var file = NxtFormat.ForBitsPerPixel(bits).Read([]);
        file.Entries.Should().BeEmpty();
        file.Tiles.Count.Should().Equal(0);
        file.ToByteArray().Should().BeEmpty();
    }
    [Test]
    public void Read_ExplicitInterpretation()
    {
        var bytes = TileTestData.Bytes(8, 8, 1);
        var four = NxtFormat.FourBit.Read(bytes);
        var eight = NxtFormat.EightBit.Read(bytes);
        four.Tiles.Count.Should().Equal(2);
        eight.Tiles.Count.Should().Equal(1);
        four.Tiles.BitsPerPixel.Should().Equal(4);
        eight.Tiles.BitsPerPixel.Should().Equal(8);
        four.ToByteArray().Should().SequenceEqual(bytes);
        eight.ToByteArray().Should().SequenceEqual(bytes);
    }
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(8)]
    public void Read_InvalidLength(int bits) =>
        AssertThat.Invoking(() => NxtFormat.ForBitsPerPixel(bits).Read(new byte[64 * bits / 8 - 1])).Should().Throw<InvalidDataException>();
    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(new byte[32]);
        await stream.Awaiting(input => NxtFormat.FourBit.ReadAsync(input, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }
    [Test]
    public async Task WriteAsync_Cancelled()
    {
        var file = NxtFormat.FourBit.Read(new byte[32]);
        using var stream = new MemoryStream();
        await stream.Awaiting(output => file.WriteAsync(output, cancellationToken: new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }
}