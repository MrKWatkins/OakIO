using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Jasc;

namespace MrKWatkins.OakIO.Tests.Resources.Jasc;

public sealed class JascColourEntryTests
{
    [Test]
    public void Constructor()
    {
        const string text = "  1\t2  3  \r\n";
        var entry = JascFormat.Instance.Read(Encoding.UTF8.GetBytes("JASC-PAL\n0100\n1\n" + text)).Entries[0];
        entry.Colour.Should().Equal(new Colour(1, 2, 3));
        entry.Text.Should().Equal("  1\t2  3  ");
        entry.LineEnding.Should().Equal("\r\n");
        entry.Data.Should().SequenceEqual(Encoding.UTF8.GetBytes(text));
    }
}