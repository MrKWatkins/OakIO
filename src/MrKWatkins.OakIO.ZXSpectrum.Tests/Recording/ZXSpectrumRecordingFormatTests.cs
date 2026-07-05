using MrKWatkins.OakIO.ZXSpectrum.Recording;
using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;
using MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording;

public sealed class ZXSpectrumRecordingFormatTests
{
    [Pure]
    private static byte[] CreateBytes() => RzxFormatTests.CreateRzxFile().ToByteArray();

    [Test]
    public void Read_ByteArray_NonGeneric()
    {
        var bytes = CreateBytes();
        ZXSpectrumRecordingFormat format = RzxFormat.Instance;

        // No cast required: proves ZXSpectrumRecordingFormat.Read(byte[]) hides the base IOFile-returning overload.
        ZXSpectrumRecordingFile file = format.Read(bytes);

        file.Should().BeOfType<RzxFile>();
    }

    [Test]
    public void Read_Stream_NonGeneric()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumRecordingFormat format = RzxFormat.Instance;

        // No cast required: proves ZXSpectrumRecordingFormat.Read(Stream) hides the base IOFile-returning overload.
        ZXSpectrumRecordingFile file = format.Read(stream);

        file.Should().BeOfType<RzxFile>();
    }

    [Test]
    public async Task ReadAsync_NonGeneric()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumRecordingFormat format = RzxFormat.Instance;

        // No cast required: proves ZXSpectrumRecordingFormat.ReadAsync hides the base IOFile-returning overload.
        ZXSpectrumRecordingFile file = await format.ReadAsync(stream);

        file.Should().BeOfType<RzxFile>();
    }

    [Test]
    public void Read_ByteArray_Generic()
    {
        var bytes = CreateBytes();
        ZXSpectrumRecordingFormat<RzxFile> format = RzxFormat.Instance;

        // No cast required: proves ZXSpectrumRecordingFormat<TFile>.Read(byte[]) hides the ZXSpectrumRecordingFile-returning overload.
        RzxFile file = format.Read(bytes);

        file.Blocks.Should().HaveCount(3);
    }

    [Test]
    public void Read_Stream_Generic()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumRecordingFormat<RzxFile> format = RzxFormat.Instance;

        // No cast required: proves ZXSpectrumRecordingFormat<TFile>.Read(Stream) hides the ZXSpectrumRecordingFile-returning overload.
        RzxFile file = format.Read(stream);

        file.Blocks.Should().HaveCount(3);
    }

    [Test]
    public async Task ReadAsync_Generic()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumRecordingFormat<RzxFile> format = RzxFormat.Instance;

        // No cast required: proves ZXSpectrumRecordingFormat<TFile>.ReadAsync hides the ZXSpectrumRecordingFile-returning overload.
        RzxFile file = await format.ReadAsync(stream);

        file.Blocks.Should().HaveCount(3);
    }
}