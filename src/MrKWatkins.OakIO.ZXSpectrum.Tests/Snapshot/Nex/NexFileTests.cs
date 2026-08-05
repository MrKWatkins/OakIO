using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Nex;

public sealed class NexFileTests
{
    [Test]
    public void CreateCode()
    {
        var code = new byte[] { 0xF3, 0xAF, 0x76 };

        var file = NexFile.CreateCode([(2, code)], pc: 0x8000, sp: 0xFEFE, ZXColour.Blue, NexRamRequired.Ram1792K);

        file.Header.PC.Should().Equal(0x8000);
        file.Header.SP.Should().Equal(0xFEFE);
        file.Header.BorderColour.Should().Equal(ZXColour.Blue);
        file.Header.RamRequired.Should().Equal(NexRamRequired.Ram1792K);
        file.Header.NumBanksToLoad.Should().Equal(1);
        file.Header.IsBankIncluded(2).Should().BeTrue();
        file.Header.IsBankIncluded(5).Should().BeFalse();

        file.Screens.Should().BeEmpty();
        file.Palette.Should().BeNull();
        file.CopperCode.Should().BeNull();

        file.Banks.Should().HaveCount(1);
        var bank = file.Banks[0];
        bank.BankNumber.Should().Equal(2);
        bank.Data.Count.Should().Equal(16384);
        bank.Data.Take(3).SequenceEqual(code).Should().BeTrue();
        bank.Data[3].Should().Equal(0);
    }

    [Test]
    public async Task CreateCode_RoundTrips()
    {
        var code = new byte[] { 0xF3, 0xAF, 0x76 };
        var file = NexFile.CreateCode([(5, [0x05]), (2, code)], pc: 0x9234, sp: 0x7FFE);

        using var stream = new MemoryStream();
        using (var writer = new SyncStreamBinaryWriter(stream))
        {
            await NexFormat.Instance.WriteAsync(file, writer);
        }

        stream.Position = 0;
        var read = await NexFormat.Instance.ReadAsync(stream);

        read.Header.PC.Should().Equal(0x9234);
        read.Header.SP.Should().Equal(0x7FFE);
        read.Header.NumBanksToLoad.Should().Equal(2);
        read.Banks.Should().HaveCount(2);
        read.Banks[0].BankNumber.Should().Equal(5);
        read.Banks[0].Data[0].Should().Equal(0x05);
        read.Banks[1].BankNumber.Should().Equal(2);
        read.Banks[1].Data.Take(3).SequenceEqual(code).Should().BeTrue();
    }

    [Test]
    public void CreateCode_BankTooBig()
    {
        AssertThat.Invoking(() => NexFile.CreateCode([(2, new byte[16385])], pc: 0x8000, sp: 0xFEFE))
            .Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateCode_DuplicateBank()
    {
        AssertThat.Invoking(() => NexFile.CreateCode([(2, [0x01]), (2, [0x02])], pc: 0x8000, sp: 0xFEFE))
            .Should().Throw<ArgumentException>();
    }
}