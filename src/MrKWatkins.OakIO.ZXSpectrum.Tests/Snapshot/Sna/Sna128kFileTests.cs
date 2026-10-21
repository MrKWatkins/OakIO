using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Sna;

public sealed class Sna128kFileTests
{
    [Test]
    public void Create([Range(0, 7)] int selected, [Values] bool trDos)
    {
        var data = Enumerable.Range(0, 8).Select(bank => Enumerable.Repeat((byte)(bank + 17), 16384).ToArray()).ToArray();
        var inputs = data.Select(bank => (ReadOnlyMemory<byte>)bank).ToArray();
        var port = (byte)(0x38 | selected);
        var snapshot = Sna128kFile.Create(inputs, port, trDos);
        snapshot.Registers.PC = 0x8123;
        snapshot.Registers.SP = 0xBFFE;
        foreach (var bank in data)
        {
            bank.AsSpan().Clear();
        }
        inputs.AsSpan().Clear();
        snapshot.Port7FFD.Should().Equal(port);
        snapshot.TrDosRomPaged.Should().Equal(trDos);
        var bytes = snapshot.ToByteArray();
        bytes.Length.Should().Equal(selected is 2 or 5 ? 147487 : 131103);
        bytes.GetUInt16(23).Should().Equal((ushort)0xBFFE);
        bytes.GetUInt16(49179).Should().Equal((ushort)0x8123);
        bytes[49181].Should().Equal(port);
        bytes[49182].Should().Equal(trDos ? (byte)1 : (byte)0);
        bytes.AsSpan(27, 16384).ToArray().Should().OnlyContain(value => value == 22);
        bytes.AsSpan(16411, 16384).ToArray().Should().OnlyContain(value => value == 19);
        bytes.AsSpan(32795, 16384).ToArray().Should().OnlyContain(value => value == selected + 17);
        var remaining = new[] { 0, 1, 3, 4, 6, 7 }.Where(bank => bank != selected).ToArray();
        for (var index = 0; index < remaining.Length; index++)
        {
            bytes.AsSpan(49183 + index * 16384, 16384).ToArray().Should().SequenceEqual(Enumerable.Repeat((byte)(remaining[index] + 17), 16384));
        }
        var read = SnaFormat.Instance.Read(bytes).Should().BeOfType<Sna128kFile>().Value;
        for (var bank = 0; bank < 8; bank++)
        {
            read.GetBank(bank).ToArray().Should().SequenceEqual(Enumerable.Repeat((byte)(bank + 17), 16384));
        }
        read.Registers.PC.Should().Equal((ushort)0x8123);
        read.Registers.SP.Should().Equal((ushort)0xBFFE);
        var memory = Enumerable.Repeat((byte)0xCC, 65536).ToArray();
        read.TryLoadInto(memory).Should().BeTrue();
        memory.AsSpan(0, 16384).ToArray().Should().OnlyContain(value => value == 0xCC);
        memory.AsSpan(0x4000, 16384).ToArray().Should().OnlyContain(value => value == 22);
        memory.AsSpan(0x8000, 16384).ToArray().Should().OnlyContain(value => value == 19);
        memory.AsSpan(0xC000, 16384).ToArray().Should().OnlyContain(value => value == selected + 17);
    }

    [Test]
    public void Create_Defaults()
    {
        var file = Sna128kFile.Create(Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], 8).ToArray());
        file.Port7FFD.Should().Equal((byte)16);
        file.TrDosRomPaged.Should().BeFalse();
        file.Registers.PC.Should().Equal((ushort)0);
        file.Registers.SP.Should().Equal((ushort)0);
    }

    [Test]
    public void Create_InvalidBanks() => AssertThat.Invoking(() => Sna128kFile.Create([])).Should().Throw<ArgumentException>();

    [Test]
    public void Create_NullBanks() => AssertThat.Invoking(() => Sna128kFile.Create(null!)).Should().Throw<ArgumentNullException>();

    [TestCase(-1)]
    [TestCase(8)]
    public void GetBank_InvalidBank(int bank)
    {
        var file = Sna128kFile.Create(Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], 8).ToArray());
        AssertThat.Invoking(() => file.GetBank(bank).ToArray()).Should().Throw<ArgumentOutOfRangeException>();
    }
}