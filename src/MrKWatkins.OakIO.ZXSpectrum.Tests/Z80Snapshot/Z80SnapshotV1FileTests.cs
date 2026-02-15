using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Z80Snapshot;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80SnapshotV1FileTests
{
    [Test]
    public void Create48k([Values] bool compress)
    {
        var memory = new byte[65536];
        TestContext.CurrentContext.Random.NextBytes(memory.AsSpan()[16384..]);

        var snapshot = Z80SnapshotV1File.Create48k(memory, compress);

        var actual = new byte[65536];
        snapshot.TryLoadInto(actual).Should().BeTrue();

        actual.Should().SequenceEqual(memory);
    }
}