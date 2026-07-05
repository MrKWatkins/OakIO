using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tzx;

public sealed class ArchiveInfoTypeExtensionsTests
{
    [Test]
    public void ToDescription_WithDescriptionAttribute()
    {
        ArchiveInfoType.FullTitle.ToDescription().Should().Equal("Full Title");
    }

    [Test]
    public void ToDescription_UndefinedValue_ReturnsName()
    {
        ((ArchiveInfoType)200).ToDescription().Should().Equal("200");
    }
}