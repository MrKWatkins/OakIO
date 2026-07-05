using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Nex;

public sealed class NexBankTests
{
    [Test]
    public void Constructor()
    {
        byte[] data = [0x01, 0x02, 0x03];
        var bank = new NexBank(5, data);

        bank.BankNumber.Should().Equal(5);
        bank.Data.Should().SequenceEqual(data);
    }

    [Test]
    public void ToString_ReturnsBankNumber()
    {
        var bank = new NexBank(5, []);

        bank.ToString().Should().Equal("Bank 5");
    }
}