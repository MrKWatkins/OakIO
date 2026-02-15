namespace MrKWatkins.OakIO.Tests;

public sealed class HeaderTests
{
    [Test]
    public void Constructor_Int()
    {
        var header = new TestHeader(3);
        header.Length.Should().Equal(3);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0, 0, 0);
    }

    [Test]
    public void Constructor_Int_Stream()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var header = new TestHeader(3, stream);
        header.Length.Should().Equal(3);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Constructor_Int_IEnumerable()
    {
        var header = new TestHeader(3, (IEnumerable<byte>)[1, 2, 3]);
        header.Length.Should().Equal(3);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Constructor_ByteArray()
    {
        var header = new TestHeader([1, 2, 3]);
        header.Length.Should().Equal(3);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    private sealed class TestHeader : Header
    {
        public TestHeader(int length) : base(length)
        {
        }

        public TestHeader(int length, Stream data) : base(length, data)
        {
        }

        public TestHeader(int length, IEnumerable<byte> data) : base(length, data)
        {
        }

        public TestHeader(byte[] data) : base(data)
        {
        }
    }
}