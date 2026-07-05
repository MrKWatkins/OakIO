using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Wav;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class ZXSpectrumFileFormatTests : ZXSpectrumTestFixture
{
    [TestCase(Resources.AufWiedersehenMontyZ80V2)]
    [TestCase(Resources.AufWiedersehenMontyZ80V2Zip)]
    public void Load_String(string resource)
    {
        using var file = GetResourceAsTemporaryFile(resource);
        var snapshot = ZXSpectrumFileFormat.Load(file.Path);
        snapshot.Should().BeOfType<Z80V2File>();
    }

    [Test]
    public void Load_String_ThrowsForUnsupportedZip()
    {
        using var file = GetResourceAsTemporaryFile(Resources.UnsupportedZip);
        AssertThat.Invoking(() => ZXSpectrumFileFormat.Load(file.Path)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Load_Stream()
    {
        using var stream = OpenResource(Resources.AufWiedersehenMontyZ80V2);
        var snapshot = ZXSpectrumFileFormat.Load(Resources.AufWiedersehenMontyZ80V2, stream);
        snapshot.Should().BeOfType<Z80V2File>();
    }

    [Test]
    public async Task LoadAsync_String()
    {
        using var file = GetResourceAsTemporaryFile(Resources.AufWiedersehenMontyZ80V2);
        var snapshot = await ZXSpectrumFileFormat.LoadAsync(file.Path);
        snapshot.Should().BeOfType<Z80V2File>();
    }

    [Test]
    public async Task LoadAsync_Stream()
    {
        await using var stream = OpenResource(Resources.AufWiedersehenMontyZ80V2);
        var snapshot = await ZXSpectrumFileFormat.LoadAsync(Resources.AufWiedersehenMontyZ80V2, stream);
        snapshot.Should().BeOfType<Z80V2File>();
    }

    [Test]
    public void Read_ByteArray()
    {
        using var stream = OpenResource(Resources.AufWiedersehenMontyZ80V2);
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();

        ZXSpectrumFileFormat format = Z80Format.Instance;

        // No cast required: proves ZXSpectrumFileFormat.Read(byte[]) hides the base IOFile-returning overload.
        ZXSpectrumFile file = format.Read(bytes);

        file.Should().BeOfType<Z80V2File>();
    }

    [Test]
    public void Read_Stream()
    {
        using var stream = OpenResource(Resources.AufWiedersehenMontyZ80V2);

        ZXSpectrumFileFormat format = Z80Format.Instance;

        // No cast required: proves ZXSpectrumFileFormat.Read(Stream) hides the base IOFile-returning overload.
        ZXSpectrumFile file = format.Read(stream);

        file.Should().BeOfType<Z80V2File>();
    }

    [Test]
    public async Task ReadAsync()
    {
        await using var stream = OpenResource(Resources.AufWiedersehenMontyZ80V2);

        ZXSpectrumFileFormat format = Z80Format.Instance;

        // No cast required: proves ZXSpectrumFileFormat.ReadAsync hides the base IOFile-returning overload.
        ZXSpectrumFile file = await format.ReadAsync(stream);

        file.Should().BeOfType<Z80V2File>();
    }

    [Test]
    public void Constructor_FileTypeIsZXSpectrumFile_Throws()
    {
        AssertThat.Invoking(() => _ = new StubFormat(typeof(ZXSpectrumFile))).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_FileTypeIsNotZXSpectrumFile_Throws()
    {
        AssertThat.Invoking(() => _ = new StubFormat(typeof(WavFile))).Should().Throw<ArgumentException>();
    }

    private sealed class StubFormat(Type fileType) : ZXSpectrumFileFormat("Stub", "stub", fileType)
    {
        protected override ValueTask<IOFile> ReadAsync(IBinaryReader reader) => throw new NotSupportedException();

        protected internal override ValueTask WriteAsync(IOFile file, IBinaryWriter writer) => throw new NotSupportedException();
    }
}