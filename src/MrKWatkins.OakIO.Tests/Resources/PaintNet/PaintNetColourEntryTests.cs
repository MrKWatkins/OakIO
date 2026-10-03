using System.Text;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.Tests.Resources.PaintNet;

public sealed class PaintNetColourEntryTests
{
    [TestCase("  00010203  \r\n", 0)]
    [TestCase("80010203", 128)]
    [TestCase("ff010203\n", 255)]
    public void Constructor(string stored, int alpha)
    {
        var entry = PaintNetFormat.Instance.Read(Encoding.UTF8.GetBytes(stored)).Entries[0];
        entry.Colour.Should().Equal(new Colour(1, 2, 3, (byte)alpha));
        entry.Data.Should().SequenceEqual(Encoding.UTF8.GetBytes(stored));
    }
}