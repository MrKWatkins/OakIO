using MrKWatkins.OakIO.ZXSpectrum.Snapshot;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot;

public sealed class SnapshotBanksTests
{
    [Test]
    public void Validate() => AssertThat.Invoking(() => SnapshotBanks.Validate(Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], 8).ToArray()))
        .Should().NotThrow();

    [Test]
    public void Validate_Null() => AssertThat.Invoking(() => SnapshotBanks.Validate(null!)).Should().Throw<ArgumentNullException>();

    [TestCase(0)]
    [TestCase(7)]
    [TestCase(9)]
    public void Validate_InvalidCount(int count) =>
        AssertThat.Invoking(() => SnapshotBanks.Validate(Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], count).ToArray()))
            .Should().Throw<ArgumentException>();

    [TestCase(0)]
    [TestCase(16383)]
    [TestCase(16385)]
    public void Validate_InvalidSize(int size)
    {
        var banks = Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], 8).ToArray();
        banks[7] = new byte[size];
        AssertThat.Invoking(() => SnapshotBanks.Validate(banks)).Should().Throw<ArgumentException>();
    }
}