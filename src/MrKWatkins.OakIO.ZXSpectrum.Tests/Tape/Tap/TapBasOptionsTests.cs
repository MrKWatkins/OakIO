using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class TapBasOptionsTests
{
    [Test]
    public void Constructor()
    {
        var options = new TapBasOptions();
        options.ClearAddress.Should().Equal((ushort)0x7FFF);
        options.EntryPoint.Should().BeNull();
        options.EntryBank.Should().BeNull();
    }
}