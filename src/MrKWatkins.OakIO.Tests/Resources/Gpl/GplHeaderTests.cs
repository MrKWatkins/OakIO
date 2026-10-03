using System.Text;
using MrKWatkins.OakIO.Resources.Gpl;

namespace MrKWatkins.OakIO.Tests.Resources.Gpl;

public sealed class GplHeaderTests
{
    [TestCase("GIMP Palette\n", null, false, 0)]
    [TestCase("GIMP Palette\nName: \n", "", false, 0)]
    [TestCase("GIMP Palette\nName: Palette\n", "Palette", false, 0)]
    [TestCase("GIMP Palette\nName:   名  \r\nColumns: 255\n", "名", true, 255)]
    public void Constructor(string stored, string? name, bool hasColumns, int columns)
    {
        var bytes = Encoding.UTF8.GetBytes(stored);
        var header = GplFormat.Instance.Read(Encoding.UTF8.GetBytes(stored + "1 2 3\n")).Header;
        header.Signature.Should().Equal("GIMP Palette");
        header.Name.Should().Equal(name);
        header.HasColumns.Should().Equal(hasColumns);
        header.Columns.Should().Equal(columns);
        header.Data.Should().SequenceEqual(bytes);
    }
}