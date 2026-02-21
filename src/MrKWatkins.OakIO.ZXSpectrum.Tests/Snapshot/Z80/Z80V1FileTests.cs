using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80V1FileTests
{
    [Test]
    public void Create48k([Values] bool compress)
    {
        var memory = new byte[65536];
        TestContext.CurrentContext.Random.NextBytes(memory.AsSpan()[16384..]);

        var snapshot = Z80V1File.Create48k(memory, compress);

        var actual = new byte[65536];
        snapshot.TryLoadInto(actual).Should().BeTrue();

        actual.Should().SequenceEqual(memory);
    }
}