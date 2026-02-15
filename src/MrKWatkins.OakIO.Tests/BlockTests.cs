using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.Tests;

public sealed class BlockTests
{
    [Test]
    public void Constructor_Int()
    {
        var block = new TestBlock(new TestHeader(), 3);
        block.Length.Should().Equal(3);
        block.AsReadOnlySpan().ToArray().Should().SequenceEqual(0, 0, 0);
        block.Header.Should().BeOfType<TestHeader>();
        block.Trailer.Should().BeTheSameInstanceAs(EmptyTrailer.Instance);
    }

    [Test]
    public void Constructor_Stream()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var block = new TestBlock(new TestHeader(), 3, stream);
        block.Length.Should().Equal(3);
        block.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
        block.Header.Should().BeOfType<TestHeader>();
        block.Trailer.Should().BeTheSameInstanceAs(EmptyTrailer.Instance);
    }

    [Test]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public void Constructor_IEnumerable()
    {
        IEnumerable<byte> bytes = [1, 2, 3];
        var block = new TestBlock(new TestHeader(), 3, bytes);
        block.Data.ToArray().Should().SequenceEqual(bytes);
        block.Length.Should().Equal(3);
        block.AsReadOnlySpan().ToArray().Should().SequenceEqual(bytes);
        block.Header.Should().BeOfType<TestHeader>();
        block.Trailer.Should().BeTheSameInstanceAs(EmptyTrailer.Instance);
    }

    [Test]
    public void Constructor_ByteArray()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var block = new TestBlock(new TestHeader(), bytes);
        block.Data.ToArray().Should().SequenceEqual(bytes);
        block.Length.Should().Equal(3);
        block.AsReadOnlySpan().ToArray().Should().SequenceEqual(bytes);
        block.Header.Should().BeOfType<TestHeader>();
        block.Trailer.Should().BeTheSameInstanceAs(EmptyTrailer.Instance);
    }

    [Test]
    public void Write()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var block = new TestBlock(new TestHeader(), bytes);

        using var stream = new MemoryStream();
        block.Write(stream);

        stream.ToArray().Should().SequenceEqual(bytes);
    }

    [Test]
    public void LoadInto_CannotLoad()
    {
        var block = new TestBlock(new TestHeader(), [1, 2, 3], false);
        var buffer = new byte[8];
        block.Invoking(b => b.LoadInto(buffer)).Should().Throw<IOException>();
    }

    [Test]
    public void LoadInto_CanLoad()
    {
        var block = new TestBlock(new TestHeader(), [1, 2, 3]);
        var buffer = new byte[8];
        block.LoadInto(buffer);
        buffer.ToArray().Should().SequenceEqual(0, 0, 0, 1, 2, 3, 0, 0);
    }

    [Test]
    public void TryLoadInto_CannotLoad()
    {
        var block = new TestBlock(new TestHeader(), [1, 2, 3], false);
        var buffer = new byte[8];
        block.TryLoadInto(buffer).Should().BeFalse();
    }

    [Test]
    public void TryLoadInto_CanLoad()
    {
        var block = new TestBlock(new TestHeader(), [1, 2, 3]);
        var buffer = new byte[8];
        block.TryLoadInto(buffer).Should().BeTrue();
        buffer.ToArray().Should().SequenceEqual(0, 0, 0, 1, 2, 3, 0, 0);
    }

    [Test]
    public void TryLoadInto_DefaultReturnsFalse()
    {
        var block = new NonLoadableBlock(new TestHeader(), [1, 2, 3]);
        var buffer = new byte[8];
        block.TryLoadInto(buffer).Should().BeFalse();
    }

    [Test]
    public void TypedHeader()
    {
        var header = new TestHeader();
        var block = new TestBlock(header, [1, 2, 3]);

        block.Header.Should().BeTheSameInstanceAs(header);

        Block typedBlock = block;
        typedBlock.Header.Should().BeTheSameInstanceAs(header);
    }

    [Test]
    public void TypedTrailer()
    {
        var block = new TestBlock(new TestHeader(), [1, 2, 3]);

        block.Trailer.Should().BeTheSameInstanceAs(EmptyTrailer.Instance);

        Block typedBlock = block;
        typedBlock.Trailer.Should().BeTheSameInstanceAs(EmptyTrailer.Instance);
    }

    [Test]
    public void BlockWithTrailer_Constructor_Int()
    {
        var header = new TestHeader();
        var trailer = new TestTrailer([0xAB]);
        var block = new TestBlockWithTrailer(header, trailer, 3);
        block.Length.Should().Equal(3);
        block.Header.Should().BeTheSameInstanceAs(header);
        block.Trailer.Should().BeTheSameInstanceAs(trailer);
    }

    [Test]
    public void BlockWithTrailer_Constructor_Stream()
    {
        var header = new TestHeader();
        var trailer = new TestTrailer([0xAB]);
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var block = new TestBlockWithTrailer(header, trailer, 3, stream);
        block.Length.Should().Equal(3);
        block.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
        block.Header.Should().BeTheSameInstanceAs(header);
        block.Trailer.Should().BeTheSameInstanceAs(trailer);
    }

    [Test]
    public void BlockWithTrailer_Constructor_IEnumerable()
    {
        var header = new TestHeader();
        var trailer = new TestTrailer([0xAB]);
        var block = new TestBlockWithTrailer(header, trailer, 3, [1, 2, 3]);
        block.Length.Should().Equal(3);
        block.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
        block.Header.Should().BeTheSameInstanceAs(header);
        block.Trailer.Should().BeTheSameInstanceAs(trailer);
    }

    [Test]
    public void BlockWithTrailer_Constructor_ByteArray()
    {
        var header = new TestHeader();
        var trailer = new TestTrailer([0xAB]);
        var block = new TestBlockWithTrailer(header, trailer, [1, 2, 3]);
        block.Length.Should().Equal(3);
        block.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
        block.Header.Should().BeTheSameInstanceAs(header);
        block.Trailer.Should().BeTheSameInstanceAs(trailer);
    }

    private sealed class TestBlock : Block<TestHeader>
    {
        private readonly bool canLoad;

        public TestBlock(Header header, int length)
            : base(header, length)
        {
        }

        public TestBlock(Header header, int length, Stream data)
            : base(header, length, data)
        {
        }

        public TestBlock(Header header, int length, [InstantHandle] IEnumerable<byte> data)
            : base(header, length, data)
        {
        }

        public TestBlock(Header header, byte[] data, bool canLoad = true)
            : base(header, data)
        {
            this.canLoad = canLoad;
        }

        public override bool TryLoadInto(Span<byte> memory)
        {
            if (canLoad)
            {
                Data.CopyTo(memory[3..]);
                return true;
            }

            return false;
        }
    }

    private sealed class NonLoadableBlock(Header header, byte[] data) : Block<TestHeader>(header, data);

    private sealed class TestBlockWithTrailer : Block<TestHeader, TestTrailer>
    {
        public TestBlockWithTrailer(TestHeader header, TestTrailer trailer, int length)
            : base(header, trailer, length)
        {
        }

        public TestBlockWithTrailer(TestHeader header, TestTrailer trailer, int length, Stream data)
            : base(header, trailer, length, data)
        {
        }

        public TestBlockWithTrailer(TestHeader header, TestTrailer trailer, int length, [InstantHandle] IEnumerable<byte> data)
            : base(header, trailer, length, data)
        {
        }

        public TestBlockWithTrailer(TestHeader header, TestTrailer trailer, byte[] data)
            : base(header, trailer, data)
        {
        }
    }

    private sealed class TestHeader() : Header(5);

    private sealed class TestTrailer(byte[] data) : Trailer(data);
}