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
}