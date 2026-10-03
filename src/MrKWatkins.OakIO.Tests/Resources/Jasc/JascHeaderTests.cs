using System.Text;
using MrKWatkins.OakIO.Resources.Jasc;

namespace MrKWatkins.OakIO.Tests.Resources.Jasc;

public sealed class JascHeaderTests
{
    [Test]
    public void Constructor()
    {
        const string text = "\uFEFFJASC-PAL\r\n0100\r\n 2 \r\n";
        var header = JascFormat.Instance.Read(Encoding.UTF8.GetBytes(text + "1 2 3\n255 128 0")).Header;
        header.Signature.Should().Equal("JASC-PAL");
        header.Version.Should().Equal("0100");
        header.ColourCount.Should().Equal(2);
        header.Data.Should().SequenceEqual(Encoding.UTF8.GetBytes(text));
    }
}