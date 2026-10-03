using System.Text;
using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Tests.Resources.Text;

public sealed class TextLineTests
{
    [TestCase("", "", "")]
    [TestCase("hello", "hello", "")]
    [TestCase("hello\n", "hello", "\n")]
    [TestCase("hello\r\n", "hello", "\r\n")]
    [TestCase("hello\r", "hello", "\r")]
    [TestCase("\uFEFFTéšt\r\n", "Téšt", "\r\n")]
    public void Constructor(string stored, string text, string ending)
    {
        var bytes = Encoding.UTF8.GetBytes(stored);
        var line = new TextLine(bytes);
        line.Text.Should().Equal(text);
        line.LineEnding.Should().Equal(ending);
        line.Data.Should().SequenceEqual(bytes);
    }
}