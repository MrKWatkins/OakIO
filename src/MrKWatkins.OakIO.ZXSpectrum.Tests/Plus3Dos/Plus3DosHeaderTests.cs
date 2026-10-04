using MrKWatkins.OakIO.ZXSpectrum.Plus3Dos;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Plus3Dos;

public sealed class Plus3DosHeaderTests
{
    [TestCase(0)]
    [TestCase(49152)]
    [TestCase(81920)]
    public void Create(int length)
    {
        var header = Plus3DosHeader.Create(length);
        header.Length.Should().Equal(128);
        header.Signature.Should().Equal("PLUS3DOS");
        header.Issue.Should().Equal((byte)1);
        header.Version.Should().Equal((byte)0);
        header.FileSize.Should().Equal((uint)length + 128);
        header.FileType.Should().Equal((byte)3);
        header.BasicLength.Should().Equal(unchecked((ushort)length));
        header.Parameter1.Should().Equal((ushort)16384);
        header.Parameter2.Should().Equal((ushort)32768);
        header.Checksum.Should().Equal(unchecked((byte)header.Data.Take(127).Sum(value => value)));
    }

    [Test]
    public void Constructor_ByteArray()
    {
        var bytes = Plus3DosHeader.Create(49152).Data.ToArray();
        bytes[126] = 42;
        bytes[127] = unchecked((byte)bytes.Take(127).Sum(value => value));
        var header = new Plus3DosHeader(bytes);
        header.Data.Should().SequenceEqual(bytes);
        bytes[126] = 0;
        header.Data[126].Should().Equal((byte)42);
    }

    [TestCase(0)]
    [TestCase(8)]
    [TestCase(127)]
    public void Constructor_ByteArray_Invalid(int index)
    {
        var bytes = Plus3DosHeader.Create(0).Data.ToArray();
        bytes[index] ^= 255;
        AssertThat.Invoking(() => new Plus3DosHeader(bytes)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Create_Negative() =>
        AssertThat.Invoking(() => Plus3DosHeader.Create(-1)).Should().Throw<ArgumentOutOfRangeException>();

    [Test]
    public void Constructor_ByteArray_Null() =>
        AssertThat.Invoking(() => new Plus3DosHeader(null!)).Should().Throw<ArgumentNullException>();

    [TestCase(127)]
    [TestCase(129)]
    public void Constructor_ByteArray_InvalidLength(int length) =>
        AssertThat.Invoking(() => new Plus3DosHeader(new byte[length])).Should().Throw<ArgumentException>();
}