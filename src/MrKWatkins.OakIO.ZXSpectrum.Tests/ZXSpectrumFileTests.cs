using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public sealed class ZXSpectrumFileTests : ZXSpectrumTestFixture
{
    [TestCase(Resources.AufWiedersehenMontyZ80V2)]
    [TestCase(Resources.AufWiedersehenMontyZ80V2Zip)]
    public void Read_String(string resource)
    {
        using var file = GetResourceAsTemporaryFile(resource);
        var snapshot = ZXSpectrumFile.Read(file.Path);
        snapshot.Should().BeOfType<Z80SnapshotV2File>();
    }

    [Test]
    public void Read_String_ThrowsForUnsupportedZip()
    {
        using var file = GetResourceAsTemporaryFile(Resources.UnsupportedZip);
        AssertThat.Invoking(() => ZXSpectrumFile.Read(file.Path)).Should().Throw<NotSupportedException>();
    }

    [TestCase(Resources.AufWiedersehenMontyZ80V2)]
    [TestCase(Resources.AufWiedersehenMontyZ80V2Zip)]
    public void Read_String_Stream(string resource)
    {
        using var monty = OpenResource(resource);
        var snapshot = ZXSpectrumFile.Read(resource, monty);
        snapshot.Should().BeOfType<Z80SnapshotV2File>();
    }

    [Test]
    public void Read_String_Stream_ThrowsForUnsupportedSnapshotType()
    {
        using var monty = OpenResource(Resources.AufWiedersehenMontyZ80V2);
        AssertThat.Invoking(() => ZXSpectrumFile.Read("monty.blah", monty)).Should().Throw<NotSupportedException>();
    }
}