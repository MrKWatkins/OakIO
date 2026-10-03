using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Gpl;

namespace MrKWatkins.OakIO.Tests.Resources.Gpl;

public sealed class GplColourEntryTests
{
    [TestCase(" 1\t2 3 名称 \r\n", "名称", "\r\n")]
    [TestCase("1 2 3", "", "")]
    public void Constructor(string stored, string name, string ending)
    {
        var entry = GplFormat.Instance.Read(Encoding.UTF8.GetBytes("GIMP Palette\n" + stored)).Entries[0];
        entry.Colour.Should().Equal(new Colour(1, 2, 3));
        entry.Name.Should().Equal(name);
        entry.LineEnding.Should().Equal(ending);
        entry.Data.Should().SequenceEqual(Encoding.UTF8.GetBytes(stored));
    }
}