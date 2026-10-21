using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Snapshot.Z80;

public sealed class Z80V2OrV3FileTests
{
    [TestCase(-1)]
    [TestCase(8)]
    public void GetBank_InvalidBank(int bank)
    {
        var file = Z80V2File.Create128k(Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], 8).ToArray());
        AssertThat.Invoking(() => file.GetBank(bank)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetBank_48k()
    {
        var file = Z80V2File.Create48k(new byte[65536]);
        AssertThat.Invoking(() => file.GetBank(0)).Should().Throw<InvalidOperationException>();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GetBank_MissingOrDuplicate(bool duplicate)
    {
        var full = Z80V2File.Create128k(Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[16384], 8).ToArray());
        var pages = duplicate ? full.Pages.Append(full.Pages[0]) : full.Pages.Skip(1);
        var file = new Z80V2File(full.Header, pages);
        AssertThat.Invoking(() => file.GetBank(0)).Should().Throw<InvalidDataException>();
    }
}