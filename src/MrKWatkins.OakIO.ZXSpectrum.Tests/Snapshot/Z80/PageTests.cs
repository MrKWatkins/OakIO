using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

public sealed class PageTests
{
    [Test]
    public void Create128k_InvalidBanks() => AssertThat.Invoking(() => Page.Create128k([])).Should().Throw<ArgumentException>();

    [Test]
    public void Create128k_NullBanks() => AssertThat.Invoking(() => Page.Create128k(null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Create128k([Values] bool compress)
    {
        var banks = Enumerable.Range(0, 8).Select(bank => Enumerable.Repeat((byte)(bank + 17), 16384).ToArray()).ToArray();
        // Exercise both marker bytes and incompressible data in addition to long runs.
        TestContext.CurrentContext.Random.NextBytes(banks[3]);
        banks[6].AsSpan().Fill(0xED);
        var expected = banks.Select(bank => bank.ToArray()).ToArray();
        var pages = Page.Create128k(banks.Select(bank => (ReadOnlyMemory<byte>)bank).ToArray(), compress);
        foreach (var bank in banks)
        {
            bank.AsSpan().Clear();
        }
        pages.Count.Should().Equal(8);
        for (var bank = 0; bank < 8; bank++)
        {
            pages[bank].Header.PageNumber.Should().Equal((byte)(bank + 3));
            pages[bank].Header.HardwareMode.Should().Equal(HardwareMode.Spectrum128);
            pages[bank].UncompressedData.Should().SequenceEqual(expected[bank]);
            if (!compress)
            {
                pages[bank].Header.CompressedLength.Should().Equal((ushort)65535);
                pages[bank].Data.Should().SequenceEqual(expected[bank]);
            }
        }
    }
}