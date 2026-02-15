namespace MrKWatkins.OakIO.Tests;

public sealed class TrailerTests
{
    [Test]
    public void Constructor_Int()
    {
        var trailer = new TestTrailer(3);
        trailer.Length.Should().Equal(3);
        trailer.AsReadOnlySpan().ToArray().Should().SequenceEqual(0, 0, 0);
    }

    [Test]
    public void Constructor_Int_Stream()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var trailer = new TestTrailer(3, stream);
        trailer.Length.Should().Equal(3);
        trailer.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Constructor_Int_IEnumerable()
    {
        var trailer = new TestTrailer(3, (IEnumerable<byte>)[1, 2, 3]);
        trailer.Length.Should().Equal(3);
        trailer.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Constructor_ByteArray()
    {
        var trailer = new TestTrailer([1, 2, 3]);
        trailer.Length.Should().Equal(3);
        trailer.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    private sealed class TestTrailer : Trailer
    {
        public TestTrailer(int length) : base(length)
        {
        }

        public TestTrailer(int length, Stream data) : base(length, data)
        {
        }

        public TestTrailer(int length, IEnumerable<byte> data) : base(length, data)
        {
        }

        public TestTrailer(byte[] data) : base(data)
        {
        }
    }
}