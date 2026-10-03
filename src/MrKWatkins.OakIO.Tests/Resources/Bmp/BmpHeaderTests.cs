using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Tests.Resources.Bmp;

public sealed class BmpHeaderTests
{
    [Test]
    public void Constructor()
    {
        var header = BmpFormat.Instance.Read(BmpFormatTests.Create(24, false)).Header;
        header.Signature.Should().Equal("BM");
        header.FileSize.Should().Equal(78u);
        header.Reserved1.Should().Equal((ushort)0);
        header.Reserved2.Should().Equal((ushort)0);
        header.PixelDataOffset.Should().Equal(54u);
        header.Length.Should().Equal(14);
        header.Data.Should().SequenceEqual(BmpFormatTests.Create(24, false)[..14]);
    }
}