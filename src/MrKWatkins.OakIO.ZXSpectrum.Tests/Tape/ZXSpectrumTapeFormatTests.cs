using MrKWatkins.OakIO.ZXSpectrum.Tape;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape;

public sealed class ZXSpectrumTapeFormatTests
{
    [Pure]
    private static byte[] CreateBytes() => TapFile.CreateCode("test", 0x8000, [0xF3, 0xAF]).ToByteArray();

    [Test]
    public void Read_ByteArray_NonGeneric()
    {
        var bytes = CreateBytes();
        ZXSpectrumTapeFormat format = TapFormat.Instance;

        // No cast required: proves ZXSpectrumTapeFormat.Read(byte[]) hides the base IOFile-returning overload.
        ZXSpectrumTapeFile file = format.Read(bytes);

        file.Should().BeOfType<TapFile>();
    }

    [Test]
    public void Read_Stream_NonGeneric()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumTapeFormat format = TapFormat.Instance;

        // No cast required: proves ZXSpectrumTapeFormat.Read(Stream) hides the base IOFile-returning overload.
        ZXSpectrumTapeFile file = format.Read(stream);

        file.Should().BeOfType<TapFile>();
    }

    [Test]
    public async Task ReadAsync_NonGeneric()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumTapeFormat format = TapFormat.Instance;

        // No cast required: proves ZXSpectrumTapeFormat.ReadAsync hides the base IOFile-returning overload.
        ZXSpectrumTapeFile file = await format.ReadAsync(stream);

        file.Should().BeOfType<TapFile>();
    }

    [Test]
    public void Read_ByteArray_Generic()
    {
        var bytes = CreateBytes();
        ZXSpectrumTapeFormat<TapFile> format = TapFormat.Instance;

        // No cast required: proves ZXSpectrumTapeFormat<TFile>.Read(byte[]) hides the ZXSpectrumTapeFile-returning overload.
        TapFile file = format.Read(bytes);

        file.Blocks.Should().HaveCount(2);
    }

    [Test]
    public void Read_Stream_Generic()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumTapeFormat<TapFile> format = TapFormat.Instance;

        // No cast required: proves ZXSpectrumTapeFormat<TFile>.Read(Stream) hides the ZXSpectrumTapeFile-returning overload.
        TapFile file = format.Read(stream);

        file.Blocks.Should().HaveCount(2);
    }

    [Test]
    public async Task ReadAsync_Generic()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumTapeFormat<TapFile> format = TapFormat.Instance;

        // No cast required: proves ZXSpectrumTapeFormat<TFile>.ReadAsync hides the ZXSpectrumTapeFile-returning overload.
        TapFile file = await format.ReadAsync(stream);

        file.Blocks.Should().HaveCount(2);
    }
}