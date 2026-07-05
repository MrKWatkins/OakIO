using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot;

public sealed class ZXSpectrumSnapshotFormatTests
{
    [Pure]
    private static byte[] CreateBytes()
    {
        var memory = new byte[65536];
        return Sna48kFile.Create(memory).ToByteArray();
    }

    [Test]
    public void Read_ByteArray_NonGeneric()
    {
        var bytes = CreateBytes();
        ZXSpectrumSnapshotFormat format = SnaFormat.Instance;

        // No cast required: proves ZXSpectrumSnapshotFormat.Read(byte[]) hides the base IOFile-returning overload.
        ZXSpectrumSnapshotFile file = format.Read(bytes);

        file.Should().BeOfType<Sna48kFile>();
    }

    [Test]
    public void Read_Stream_NonGeneric()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumSnapshotFormat format = SnaFormat.Instance;

        // No cast required: proves ZXSpectrumSnapshotFormat.Read(Stream) hides the base IOFile-returning overload.
        ZXSpectrumSnapshotFile file = format.Read(stream);

        file.Should().BeOfType<Sna48kFile>();
    }

    [Test]
    public async Task ReadAsync_NonGeneric()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumSnapshotFormat format = SnaFormat.Instance;

        // No cast required: proves ZXSpectrumSnapshotFormat.ReadAsync hides the base IOFile-returning overload.
        ZXSpectrumSnapshotFile file = await format.ReadAsync(stream);

        file.Should().BeOfType<Sna48kFile>();
    }

    [Test]
    public void Read_ByteArray_Generic()
    {
        var bytes = CreateBytes();
        ZXSpectrumSnapshotFormat<SnaFile> format = SnaFormat.Instance;

        // No cast required: proves ZXSpectrumSnapshotFormat<TFile>.Read(byte[]) hides the ZXSpectrumSnapshotFile-returning overload.
        SnaFile file = format.Read(bytes);

        file.Should().BeOfType<Sna48kFile>();
    }

    [Test]
    public void Read_Stream_Generic()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumSnapshotFormat<SnaFile> format = SnaFormat.Instance;

        // No cast required: proves ZXSpectrumSnapshotFormat<TFile>.Read(Stream) hides the ZXSpectrumSnapshotFile-returning overload.
        SnaFile file = format.Read(stream);

        file.Should().BeOfType<Sna48kFile>();
    }

    [Test]
    public async Task ReadAsync_Generic()
    {
        using var stream = new MemoryStream(CreateBytes());
        ZXSpectrumSnapshotFormat<SnaFile> format = SnaFormat.Instance;

        // No cast required: proves ZXSpectrumSnapshotFormat<TFile>.ReadAsync hides the ZXSpectrumSnapshotFile-returning overload.
        SnaFile file = await format.ReadAsync(stream);

        file.Should().BeOfType<Sna48kFile>();
    }
}