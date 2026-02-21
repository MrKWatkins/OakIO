using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class TapBlockTests : TapTestFixture
{
    [Test]
    public void TryLoadInto()
    {
        using var z80Test = OpenZ80Test();

        var file = TapFormat.Instance.Read(z80Test);

        var memory = new byte[65536];
        file.TryLoadInto(memory).Should().BeTrue();

        // Data should start at 32768. The first byte should be the first byte of the data (DI, 0xF3) and not the data flag. (0xFF)
        memory[32767].Should().Equal(0x00);
        memory[32768].Should().Equal(0xF3);

        // Data should end at 47065. The byte after should be 0, not the checksum byte. (0xD0)
        memory[47065].Should().Equal(0xFF);
        memory[47066].Should().Equal(0x00);
    }

    [Test]
    public void LoadInto()
    {
        using var z80Test = OpenZ80Test();

        var file = TapFormat.Instance.Read(z80Test);

        var memory = new byte[65536];
        file.Invoking(f => f.LoadInto(memory)).Should().NotThrow();

        // Data should start at 32768. The first byte should be the first byte of the data (DI, 0xF3) and not the data flag. (0xFF)
        memory[32767].Should().Equal(0x00);
        memory[32768].Should().Equal(0xF3);

        // Data should end at 47065. The byte after should be 0, not the checksum byte. (0xD0)
        memory[47065].Should().Equal(0xFF);
        memory[47066].Should().Equal(0x00);
    }
}