using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class Z80V3FileTests
{
    [Test]
    public void Create128k([Range(0, 7)] int selected, [Values] bool compress)
    {
        var banks = Enumerable.Range(0, 8).Select(bank => (ReadOnlyMemory<byte>)Enumerable.Repeat((byte)(bank + 17), 16384).ToArray()).ToArray();
        var port = (byte)(0x38 | selected);
        var snapshot = Z80V3File.Create128k(banks, port, compress);
        snapshot.Header.HardwareMode.Should().Equal(HardwareMode.Spectrum128);
        snapshot.Port7FFD.Should().Equal(port);
        snapshot.Registers.PC = 0x8123;
        snapshot.Registers.SP = 0xBFFE;
        var bytes = snapshot.ToByteArray();
        bytes[34].Should().Equal((byte)4);
        bytes[35].Should().Equal(port);
        if (!compress)
        {
            bytes.Length.Should().Equal(87 + 8 * 16387);
            for (var bank = 0; bank < 8; bank++)
            {
                var offset = 87 + bank * 16387;
                bytes.AsSpan(offset, 3).ToArray().Should().SequenceEqual(255, 255, (byte)(bank + 3));
                bytes.AsSpan(offset + 3, 16384).ToArray().Should().SequenceEqual(banks[bank].ToArray());
            }
        }
        bytes[61].Should().Equal((byte)0xFF);
        bytes[62].Should().Equal((byte)0xFF);
        var read = Z80Format.Instance.Read(bytes).Should().BeOfType<Z80V3File>().Value;
        read.Registers.PC.Should().Equal((ushort)0x8123);
        read.Registers.SP.Should().Equal((ushort)0xBFFE);
        read.Header.HardwareMode.Should().Equal(HardwareMode.Spectrum128);
        read.Port7FFD.Should().Equal(port);
        read.Pages.Select(page => page.Header.PageNumber).Should().SequenceEqual(3, 4, 5, 6, 7, 8, 9, 10);
        for (var bank = 0; bank < 8; bank++)
        {
            read.GetBank(bank).Should().SequenceEqual(banks[bank].ToArray());
        }
        var memory = Enumerable.Repeat((byte)0xCC, 65536).ToArray();
        read.TryLoadInto(memory).Should().BeTrue();
        memory.AsSpan(0, 16384).ToArray().Should().OnlyContain(value => value == 0xCC);
        memory.AsSpan(0x4000, 16384).ToArray().Should().OnlyContain(value => value == 22);
        memory.AsSpan(0x8000, 16384).ToArray().Should().OnlyContain(value => value == 19);
        memory.AsSpan(0xC000, 16384).ToArray().Should().OnlyContain(value => value == selected + 17);
    }

    [Test]
    public void Create128k_Defaults()
    {
        var file = Z80V3File.Create128k(Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], 8).ToArray());
        file.Port7FFD.Should().Equal((byte)16);
        file.Registers.PC.Should().Equal((ushort)0);
        file.Registers.SP.Should().Equal((ushort)0);
        file.Pages.Should().OnlyContain(page => page.Header.CompressedLength != 65535);
    }

    [Test]
    public void Create128k_InvalidBanks() => AssertThat.Invoking(() => Z80V3File.Create128k([])).Should().Throw<ArgumentException>();

    [Test]
    public void Create128k_NullBanks() => AssertThat.Invoking(() => Z80V3File.Create128k(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Create48k([Values] bool compress)
    {
        var memory = new byte[65536];
        TestContext.CurrentContext.Random.NextBytes(memory.AsSpan()[16384..]);

        var snapshot = Z80V3File.Create48k(memory, compress);
        snapshot.Pages.Should().HaveCount(3);
        snapshot.Pages.Should().OnlyContain(p => p.Header.HardwareMode == HardwareMode.Spectrum48);

        var actual = new byte[65536];
        snapshot.TryLoadInto(actual).Should().BeTrue();

        actual.Should().SequenceEqual(memory);
    }
}