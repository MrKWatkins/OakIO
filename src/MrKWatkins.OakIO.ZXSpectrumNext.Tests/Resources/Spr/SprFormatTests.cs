using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Spr;

public sealed class SprFormatTests
{
    [TestCase(4)]
    [TestCase(8)]
    public void ForBitsPerPixel(int bits)
    {
        var format = SprFormat.ForBitsPerPixel(bits);
        format.BitsPerPixel.Should().Equal(bits);
        format.Name.Should().Equal($"ZX Spectrum Next Sprites ({bits}-bit)");
        format.FileExtension.Should().Equal("spr");
        format.FileType.Should().Equal(typeof(SprFile));
        SprFormat.ForBitsPerPixel(bits).Should().BeTheSameInstanceAs(format);
    }
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(2)]
    [TestCase(16)]
    public void ForBitsPerPixel_Invalid(int bits) =>
        AssertThat.Invoking(() => SprFormat.ForBitsPerPixel(bits)).Should().Throw<ArgumentOutOfRangeException>();
    [TestCase(4)]
    [TestCase(8)]
    public void Read_ByteArray(int bits)
    {
        var bytes = TileTestData.Bytes(16, bits, 2);
        var file = SprFormat.ForBitsPerPixel(bits).Read(bytes);
        file.Patterns.Count.Should().Equal(2);
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
        file.ToByteArray().Should().SequenceEqual(bytes);
    }
    [TestCase(4)]
    [TestCase(8)]
    public void Read_Stream(int bits)
    {
        var bytes = TileTestData.Bytes(16, bits, 2);
        using var stream = new MemoryStream(bytes);
        var file = SprFormat.ForBitsPerPixel(bits).Read(stream);
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
        using var output = new MemoryStream();
        file.Write(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }
    [TestCase(4)]
    [TestCase(8)]
    public async Task ReadAsync_Stream(int bits)
    {
        var bytes = TileTestData.Bytes(16, bits, 2);
        using var stream = new MemoryStream(bytes);
        var file = await SprFormat.ForBitsPerPixel(bits).ReadAsync(stream);
        file.Tiles.Pixels.Should().SequenceEqual(TileTestData.Pixels(bytes, bits));
        using var output = new MemoryStream();
        await file.WriteAsync(output);
        output.ToArray().Should().SequenceEqual(bytes);
    }
    [TestCase(4)]
    [TestCase(8)]
    public void Read_Empty(int bits)
    {
        var file = SprFormat.ForBitsPerPixel(bits).Read([]);
        file.Patterns.Should().BeEmpty();
        file.Tiles.Count.Should().Equal(0);
        file.ToByteArray().Should().BeEmpty();
    }
    [Test]
    public void Read_ExplicitInterpretation()
    {
        var bytes = TileTestData.Bytes(16, 8, 1);
        var four = SprFormat.FourBit.Read(bytes);
        var eight = SprFormat.EightBit.Read(bytes);
        four.Tiles.Count.Should().Equal(2);
        eight.Tiles.Count.Should().Equal(1);
        four.Tiles.BitsPerPixel.Should().Equal(4);
        eight.Tiles.BitsPerPixel.Should().Equal(8);
        four.ToByteArray().Should().SequenceEqual(bytes);
        eight.ToByteArray().Should().SequenceEqual(bytes);
    }
    [TestCase(4)]
    [TestCase(8)]
    public void Read_InvalidLength(int bits) =>
        AssertThat.Invoking(() => SprFormat.ForBitsPerPixel(bits).Read(new byte[256 * bits / 8 - 1])).Should().Throw<InvalidDataException>();
    [Test]
    public async Task ReadAsync_Cancelled()
    {
        using var stream = new MemoryStream(new byte[128]);
        await stream.Awaiting(input => SprFormat.FourBit.ReadAsync(input, new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }
    [Test]
    public async Task WriteAsync_Cancelled()
    {
        var file = SprFormat.FourBit.Read(new byte[128]);
        using var stream = new MemoryStream();
        await stream.Awaiting(output => file.WriteAsync(output, cancellationToken: new CancellationToken(true))).Should().ThrowAsync<OperationCanceledException>();
    }
}