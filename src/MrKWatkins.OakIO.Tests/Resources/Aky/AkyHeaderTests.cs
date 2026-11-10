using MrKWatkins.OakIO.Resources.Aky;

namespace MrKWatkins.OakIO.Tests.Resources.Aky;

public sealed class AkyHeaderTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void GetClock(bool littleEndian)
    {
        var header = new AkyFile(AkyTestData.Create(channels: 9, littleEndian: littleEndian)).Header;
        header.GetClock(0).Should().Equal(1773400u);
        header.GetClock(2).Should().Equal(1773402u);
    }

    [TestCase(AkySidProfile.Cpc, 0x80)]
    [TestCase(AkySidProfile.AmstradPlus, 0x90)]
    [TestCase(AkySidProfile.AtariSt, 0xA0)]
    [TestCase(AkySidProfile.Cpc, 0)]
    [TestCase(AkySidProfile.AmstradPlus, 0x10)]
    [TestCase(AkySidProfile.AtariSt, 0x20)]
    public void SidProfile(AkySidProfile profile, byte flags)
    {
        var bytes = AkyTestData.Create(littleEndian: (flags & 128) != 0);
        bytes[0] = (byte)(flags | 1);
        var file = new AkyFile(bytes);
        file.Header.SidProfile.Should().Equal(profile);
        file.Header.IsLittleEndian.Should().Equal((flags & 128) != 0);
        file.ToByteArray().Should().SequenceEqual(bytes);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void GetClock_InvalidChip(int chip) =>
        AssertThat.Invoking(() => new AkyFile(AkyTestData.Create(channels: 9)).Header.GetClock(chip)).Should().Throw<ArgumentOutOfRangeException>();
}