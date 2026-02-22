using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.Tests;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
[SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names")]
public sealed class IOFileComponentTests
{
    [Test]
    public void Constructor_Int()
    {
        var header = new TestIOFileComponent(3);
        header.AsReadOnlySpan().Length.Should().Equal(3);
        // ReSharper disable once UseUtf8StringLiteral
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0, 0, 0);
    }

    [Test]
    public void Constructor_Int_InvalidSize()
    {
        AssertThat.Invoking(() => new TestIOFileComponent(-1))
            .Should().ThrowArgumentOutOfRangeException("length ('-1') must be a non-negative value.", "length", -1);
    }

    [Test]
    public void Constructor_ByteArray()
    {
        var header = new TestIOFileComponent([1, 2, 3]);
        header.AsReadOnlySpan().Length.Should().Equal(3);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Constructor_Int_Stream()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var header = new TestIOFileComponent(3, stream);
        header.AsReadOnlySpan().Length.Should().Equal(3);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Constructor_Int_Stream_InvalidSize()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        AssertThat.Invoking(() => new TestIOFileComponent(-1, stream))
            .Should().ThrowArgumentOutOfRangeException("length ('-1') must be a non-negative value.", "length", -1);
    }

    [Test]
    public void Constructor_Int_Stream_StreamToShort()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        AssertThat.Invoking(() => new TestIOFileComponent(5, stream))
            .Should().ThrowArgumentException("Expected value to have at least 5 bytes, found 3.", "stream");
    }

    [Test]
    public void Constructor_Int_IEnumerable()
    {
        var header = new TestIOFileComponent(3, [1, 2, 3]);
        header.AsReadOnlySpan().Length.Should().Equal(3);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Constructor_Int_IEnumerable_InvalidSize()
    {
        AssertThat.Invoking(() => new TestIOFileComponent(-1, [1, 2, 3, 4, 5]))
            .Should().ThrowArgumentOutOfRangeException("length ('-1') must be a non-negative value.", "length", -1);
    }

    [TestCase(2)]
    [TestCase(4)]
    public void Constructor_Int_IEnumerable_IncorrectSize(int length)
    {
        AssertThat.Invoking(() => new TestIOFileComponent(length, [1, 2, 3]))
            .Should().ThrowArgumentException($"Expected value to have {length} bytes, found 3.", "bytes");
    }

    [Test]
    public void Data()
    {
        var header = new TestIOFileComponent([1, 2, 3]);
        header.Data.Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void AsReadOnlySpan_IntStart()
    {
        var header = new TestIOFileComponent([1, 2, 3, 4, 5]);
        header.AsReadOnlySpan(2).ToArray().Should().SequenceEqual(3, 4, 5);
    }

    [Test]
    public void AsReadOnlySpan_IndexStart()
    {
        var header = new TestIOFileComponent([1, 2, 3, 4, 5]);
        header.AsReadOnlySpan(^2).ToArray().Should().SequenceEqual(4, 5);
    }

    [Test]
    public void CopyTo()
    {
        var header = new TestIOFileComponent([1, 2, 3]);
        var buffer = new byte[5];
        header.CopyTo(buffer);
        buffer.Should().SequenceEqual(1, 2, 3, 0, 0);
    }

    [Test]
    public void CopyTo_WithStart()
    {
        var header = new TestIOFileComponent([1, 2, 3]);
        var buffer = new byte[5];
        header.CopyTo(buffer, 2);
        buffer.Should().SequenceEqual(0, 0, 1, 2, 3);
    }

    [Test]
    public void AsSpan()
    {
        var header = new TestIOFileComponent([1, 2, 3]);
        header.SetViaAsSpan(1, 42);
        header.GetByte(1).Should().Equal(42);
    }

    [Test]
    public void AsSpan_IntStart()
    {
        var header = new TestIOFileComponent([1, 2, 3, 4, 5]);
        header.SetViaAsSpan(2, 0, 42);
        header.GetByte(2).Should().Equal(42);
    }

    [Test]
    public void AsSpan_IndexStart()
    {
        var header = new TestIOFileComponent([1, 2, 3, 4, 5]);
        header.SetViaAsSpanFromEnd(^2, 0, 42);
        header.GetByte(3).Should().Equal(42);
    }

    [Test]
    public void Write()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var header = new TestIOFileComponent(bytes);

        using var stream = new MemoryStream();
        header.Write(stream);

        stream.ToArray().Should().SequenceEqual(bytes);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void GetByte_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetByte(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length. (3)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void GetByte_IndexOutOfRange_ByteEnum(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetByte<ByteEnum>(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length. (3)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void GetByte_IndexOutOfRange_IntEnum(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetByte<IntEnum>(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length. (3)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void SetByte_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetByte(index, 5))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length. (3)", "index", index);
    }

    [Test]
    public void GetByteAndSetByte()
    {
        var header = new TestIOFileComponent(3);
        header.GetByte(0).Should().Equal(0);
        header.GetByte(1).Should().Equal(0);
        header.GetByte(2).Should().Equal(0);

        header.SetByte(1, 5);
        header.GetByte(0).Should().Equal(0);
        header.GetByte(1).Should().Equal(5);
        header.GetByte(2).Should().Equal(0);

        header.SetByte(2, 45);
        header.GetByte(0).Should().Equal(0);
        header.GetByte(1).Should().Equal(5);
        header.GetByte(2).Should().Equal(45);

        header.SetByte(1, 3);
        header.GetByte(0).Should().Equal(0);
        header.GetByte(1).Should().Equal(3);
        header.GetByte(2).Should().Equal(45);
    }

    [Test]
    public void GetByteAndSetByte_ByteEnum()
    {
        var header = new TestIOFileComponent(3);
        header.GetByte<ByteEnum>(0).Should().Equal(ByteEnum.Zero);
        header.GetByte<ByteEnum>(1).Should().Equal(ByteEnum.Zero);
        header.GetByte<ByteEnum>(2).Should().Equal(ByteEnum.Zero);

        header.SetByte(1, ByteEnum.Two);
        header.GetByte<ByteEnum>(0).Should().Equal(ByteEnum.Zero);
        header.GetByte<ByteEnum>(1).Should().Equal(ByteEnum.Two);
        header.GetByte<ByteEnum>(2).Should().Equal(ByteEnum.Zero);

        header.SetByte(2, ByteEnum.One);
        header.GetByte<ByteEnum>(0).Should().Equal(ByteEnum.Zero);
        header.GetByte<ByteEnum>(1).Should().Equal(ByteEnum.Two);
        header.GetByte<ByteEnum>(2).Should().Equal(ByteEnum.One);

        header.SetByte(1, ByteEnum.Zero);
        header.GetByte<ByteEnum>(0).Should().Equal(ByteEnum.Zero);
        header.GetByte<ByteEnum>(1).Should().Equal(ByteEnum.Zero);
        header.GetByte<ByteEnum>(2).Should().Equal(ByteEnum.One);
    }

    [Test]
    public void GetByteAndSetByte_IntEnum()
    {
        var header = new TestIOFileComponent(3);
        header.GetByte<IntEnum>(0).Should().Equal(IntEnum.Zero);
        header.GetByte<IntEnum>(1).Should().Equal(IntEnum.Zero);
        header.GetByte<IntEnum>(2).Should().Equal(IntEnum.Zero);

        header.SetByte(1, IntEnum.Two);
        header.GetByte<IntEnum>(0).Should().Equal(IntEnum.Zero);
        header.GetByte<IntEnum>(1).Should().Equal(IntEnum.Two);
        header.GetByte<IntEnum>(2).Should().Equal(IntEnum.Zero);

        header.SetByte(2, IntEnum.One);
        header.GetByte<IntEnum>(0).Should().Equal(IntEnum.Zero);
        header.GetByte<IntEnum>(1).Should().Equal(IntEnum.Two);
        header.GetByte<IntEnum>(2).Should().Equal(IntEnum.One);

        header.SetByte(1, IntEnum.Zero);
        header.GetByte<IntEnum>(0).Should().Equal(IntEnum.Zero);
        header.GetByte<IntEnum>(1).Should().Equal(IntEnum.Zero);
        header.GetByte<IntEnum>(2).Should().Equal(IntEnum.One);
    }

    [TestCase(-1)]
    [TestCase(2)]
    public void GetWord_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetWord(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 1. (2)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(2)]
    public void SetWord_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetWord(index, 5))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 1. (2)", "index", index);
    }

    [Test]
    public void GetWordAndSetWord()
    {
        var header = new TestIOFileComponent(3);
        header.GetWord(0).Should().Equal(0);
        header.GetWord(0, Endian.Big).Should().Equal(0);
        header.GetWord(1).Should().Equal(0);
        header.GetWord(1, Endian.Big).Should().Equal(0);

        header.SetWord(1, 0x1234);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x00, 0x34, 0x12);
        header.GetWord(0).Should().Equal(0x3400);
        header.GetWord(0, Endian.Big).Should().Equal(0x0034);
        header.GetWord(1).Should().Equal(0x1234);
        header.GetWord(1, Endian.Big).Should().Equal(0x3412);

        header.SetWord(0, 0x5678, Endian.Big);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x56, 0x78, 0x12);
        header.GetWord(0).Should().Equal(0x7856);
        header.GetWord(0, Endian.Big).Should().Equal(0x5678);
        header.GetWord(1).Should().Equal(0x1278);
        header.GetWord(1, Endian.Big).Should().Equal(0x7812);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void GetUInt24_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetUInt24(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 2. (1)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void SetUInt24_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetUInt24(index, 5))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 2. (1)", "index", index);
    }

    [Test]
    public void GetUInt24AndSetUInt24()
    {
        var header = new TestIOFileComponent(4);
        header.GetUInt24(0).Should().Equal(0);
        header.GetUInt24(0, Endian.Big).Should().Equal(0);
        header.GetUInt24(1).Should().Equal(0);
        header.GetUInt24(1, Endian.Big).Should().Equal(0);

        header.SetUInt24(1, 0x123456);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x00, 0x56, 0x34, 0x12);
        header.GetUInt24(0).Should().Equal(0x345600);
        header.GetUInt24(0, Endian.Big).Should().Equal(0x005634);
        header.GetUInt24(1).Should().Equal(0x123456);
        header.GetUInt24(1, Endian.Big).Should().Equal(0x563412);

        header.SetUInt24(0, 0x56789A, Endian.Big);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x56, 0x78, 0x9A, 0x12);
        header.GetUInt24(0).Should().Equal(0x9A7856);
        header.GetUInt24(0, Endian.Big).Should().Equal(0x56789A);
        header.GetUInt24(1).Should().Equal(0x129A78);
        header.GetUInt24(1, Endian.Big).Should().Equal(0x789A12);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void GetUInt32_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(4);
        AssertThat.Invoking(() => header.GetUInt32(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 3. (1)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void SetUInt32_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(4);
        AssertThat.Invoking(() => header.SetUInt32(index, 5))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 3. (1)", "index", index);
    }

    [Test]
    public void GetUInt32AndSetUInt32()
    {
        var header = new TestIOFileComponent(5);
        header.GetUInt32(0).Should().Equal(0);
        header.GetUInt32(0, Endian.Big).Should().Equal(0);
        header.GetUInt32(1).Should().Equal(0);
        header.GetUInt32(1, Endian.Big).Should().Equal(0);

        header.SetUInt32(1, 0x12345678);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x00, 0x78, 0x56, 0x34, 0x12);
        header.GetUInt32(0).Should().Equal(0x34567800);
        header.GetUInt32(0, Endian.Big).Should().Equal(0x00785634);
        header.GetUInt32(1).Should().Equal(0x12345678);
        header.GetUInt32(1, Endian.Big).Should().Equal(0x78563412);

        header.SetUInt32(0, 0x56789ABC, Endian.Big);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x56, 0x78, 0x9A, 0xBC, 0x12);
        header.GetUInt32(0).Should().Equal(0xBC9A7856);
        header.GetUInt32(0, Endian.Big).Should().Equal(0x56789ABC);
        header.GetUInt32(1).Should().Equal(0x12BC9A78);
        header.GetUInt32(1, Endian.Big).Should().Equal(0x789ABC12);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void GetInt32_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(4);
        AssertThat.Invoking(() => header.GetInt32(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 3. (1)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void SetInt32_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(4);
        AssertThat.Invoking(() => header.SetInt32(index, 5))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 3. (1)", "index", index);
    }

    [Test]
    public void GetInt32AndSetInt32()
    {
        var header = new TestIOFileComponent(5);
        header.GetInt32(0).Should().Equal(0);
        header.GetInt32(0, Endian.Big).Should().Equal(0);
        header.GetInt32(1).Should().Equal(0);
        header.GetInt32(1, Endian.Big).Should().Equal(0);

        header.SetInt32(1, 0x12345678);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x00, 0x78, 0x56, 0x34, 0x12);
        header.GetInt32(0).Should().Equal(0x34567800);
        header.GetInt32(0, Endian.Big).Should().Equal(0x00785634);
        header.GetInt32(1).Should().Equal(0x12345678);
        header.GetInt32(1, Endian.Big).Should().Equal(0x78563412);

        header.SetInt32(0, 0x56789A01, Endian.Big);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x56, 0x78, 0x9A, 0x01, 0x12);
        header.GetInt32(0).Should().Equal(0x019A7856);
        header.GetInt32(0, Endian.Big).Should().Equal(0x56789A01);
        header.GetInt32(1).Should().Equal(0x12019A78);
        header.GetInt32(1, Endian.Big).Should().Equal(0x789A0112);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void GetInt64_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(8);
        AssertThat.Invoking(() => header.GetInt64(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 7. (1)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void SetInt64_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(8);
        AssertThat.Invoking(() => header.SetInt64(index, 5))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 7. (1)", "index", index);
    }

    [Test]
    public void GetInt64AndSetInt64()
    {
        var header = new TestIOFileComponent(9);
        header.GetInt64(0).Should().Equal(0);
        header.GetInt64(0, Endian.Big).Should().Equal(0);
        header.GetInt64(1).Should().Equal(0);
        header.GetInt64(1, Endian.Big).Should().Equal(0);

        header.SetInt64(1, 0x123456789ABCDE0FL);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x00, 0x0F, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12);
        header.GetInt64(0).Should().Equal(0x3456789ABCDE0F00L);
        header.GetInt64(0, Endian.Big).Should().Equal(0x000FDEBC9A785634L);
        header.GetInt64(1).Should().Equal(0x123456789ABCDE0FL);
        header.GetInt64(1, Endian.Big).Should().Equal(0x0FDEBC9A78563412L);

        header.SetInt64(0, 0x56789ABCDEDCBA44L, Endian.Big);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xDC, 0xBA, 0x44, 0x12);
        header.GetInt64(0).Should().Equal(0x44BADCDEBC9A7856L);
        header.GetInt64(0, Endian.Big).Should().Equal(0x56789ABCDEDCBA44L);
        header.GetInt64(1).Should().Equal(0x1244BADCDEBC9A78L);
        header.GetInt64(1, Endian.Big).Should().Equal(0x789ABCDEDCBA4412L);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void GetUInt64_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(8);
        AssertThat.Invoking(() => header.GetUInt64(index))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 7. (1)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void SetUInt64_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(8);
        AssertThat.Invoking(() => header.SetUInt64(index, 5))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length - 7. (1)", "index", index);
    }

    [Test]
    public void GetUInt64AndSetUInt64()
    {
        var header = new TestIOFileComponent(9);
        header.GetUInt64(0).Should().Equal(0);
        header.GetUInt64(0, Endian.Big).Should().Equal(0);
        header.GetUInt64(1).Should().Equal(0);
        header.GetUInt64(1, Endian.Big).Should().Equal(0);

        header.SetUInt64(1, 0x123456789ABCDEF0UL);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x00, 0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12);
        header.GetUInt64(0).Should().Equal(0x3456789ABCDEF000UL);
        header.GetUInt64(0, Endian.Big).Should().Equal(0x00F0DEBC9A785634UL);
        header.GetUInt64(1).Should().Equal(0x123456789ABCDEF0UL);
        header.GetUInt64(1, Endian.Big).Should().Equal(0xF0DEBC9A78563412UL);

        header.SetUInt64(0, 0x56789ABCDEDCBA98UL, Endian.Big);
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual(0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xDC, 0xBA, 0x98, 0x12);
        header.GetUInt64(0).Should().Equal(0x98BADCDEBC9A7856UL);
        header.GetUInt64(0, Endian.Big).Should().Equal(0x56789ABCDEDCBA98UL);
        header.GetUInt64(1).Should().Equal(0x1298BADCDEBC9A78UL);
        header.GetUInt64(1, Endian.Big).Should().Equal(0x789ABCDEDCBA9812UL);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void GetBit_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetBit(index, 0))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length. (3)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void SetBit_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetBit(index, 0, true))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length. (3)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(8)]
    public void GetBit_BitIndexOutOfRange(int bitIndex)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetBit(0, bitIndex))
            .Should().ThrowArgumentOutOfRangeException("Value must be in the range 0 -> 7.", "bitIndex", bitIndex);
    }

    [TestCase(-1)]
    [TestCase(8)]
    public void SetBit_BitIndexOutOfRange(int bitIndex)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetBit(0, bitIndex, true))
            .Should().ThrowArgumentOutOfRangeException("Value must be in the range 0 -> 7.", "bitIndex", bitIndex);
    }

    [Test]
    public void GetBitAndSetBit()
    {
        var header = new TestIOFileComponent(2);
        header.GetBit(0, 1).Should().BeFalse();
        header.GetBit(0, 7).Should().BeFalse();

        header.SetBit(1, 5, true);
        header.GetBit(1, 5).Should().BeTrue();
        header.GetBit(1, 4).Should().BeFalse();
        header.GetByte(0).Should().Equal(0b00000000);
        header.GetByte(1).Should().Equal(0b00100000);

        header.SetBit(1, 4, true);
        header.GetBit(1, 5).Should().BeTrue();
        header.GetBit(1, 4).Should().BeTrue();
        header.GetByte(0).Should().Equal(0b00000000);
        header.GetByte(1).Should().Equal(0b00110000);

        header.SetByte(0, 0b11111111);
        header.GetBit(0, 0).Should().BeTrue();
        header.GetBit(0, 7).Should().BeTrue();

        header.SetBit(0, 0, false);
        header.SetBit(0, 2, false);
        header.SetBit(0, 7, false);
        header.GetBit(0, 0).Should().BeFalse();
        header.GetBit(0, 1).Should().BeTrue();
        header.GetBit(0, 2).Should().BeFalse();
        header.GetBit(0, 3).Should().BeTrue();
        header.GetBit(0, 6).Should().BeTrue();
        header.GetBit(0, 7).Should().BeFalse();
        header.GetByte(0).Should().Equal(0b01111010);
        header.GetByte(1).Should().Equal(0b00110000);
    }

    [Test]
    public void GetBits()
    {
        var header = new TestIOFileComponent(1);
        header.SetByte(0, 0b11001010);
        header.GetBits(0, 3, 5).Should().Equal(0b00000001);
        header.GetBits(0, 6, 7).Should().Equal(0b00000011);
        header.GetBits(0, 0, 6).Should().Equal(0b01001010);
    }

    [Test]
    public void GetBits_ByteEnum()
    {
        var header = new TestIOFileComponent(1);
        header.SetByte(0, 0b11001010);
        header.GetBits<ByteEnum>(0, 3, 5).Should().Equal(ByteEnum.One);
        header.GetBits<ByteEnum>(0, 6, 7).Should().Equal(ByteEnum.Three);
    }

    [Test]
    public void GetBits_IntEnum()
    {
        var header = new TestIOFileComponent(1);
        header.SetByte(0, 0b11001010);
        header.GetBits<IntEnum>(0, 3, 5).Should().Equal(IntEnum.One);
        header.GetBits<IntEnum>(0, 6, 7).Should().Equal(IntEnum.Three);
    }

    [Test]
    public void SetBits()
    {
        var header = new TestIOFileComponent(1);
        header.SetByte(0, 0b10000001);
        header.SetBits(0, 0b11111111, 3, 5);
        header.GetByte(0).Should().Equal(0b10111001);
    }

    [Test]
    public void SetBits_ByteEnum()
    {
        var header = new TestIOFileComponent(1);
        header.SetByte(0, 0b10000001);
        header.SetBits(0, ByteEnum.Three, 3, 5);
        header.GetByte(0).Should().Equal(0b10011001);
    }

    [Test]
    public void SetBits_IntEnum()
    {
        var header = new TestIOFileComponent(1);
        header.SetByte(0, 0b10000001);
        header.SetBits(0, IntEnum.Three, 3, 5);
        header.GetByte(0).Should().Equal(0b10011001);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void GetString_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetString(index, 1))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length. (3)", "index", index);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void SetString_IndexOutOfRange(int index)
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetString(index, 1, ""))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than or equal to 0 and less than Length. (3)", "index", index);
    }

    [Test]
    public void GetString_MaximumLengthZero()
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetString(0, 0))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than 0.", "maximumLength", 0);
    }

    [Test]
    public void SetString_MaximumLengthZero()
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetString(0, 0, ""))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than 0.", "maximumLength", 0);
    }

    [Test]
    public void GetString_MaximumLengthNegative()
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetString(0, -1))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than 0.", "maximumLength", -1);
    }

    [Test]
    public void SetString_MaximumLengthNegative()
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetString(0, -1, ""))
            .Should().ThrowArgumentOutOfRangeException("Value must be greater than 0.", "maximumLength", -1);
    }

    [Test]
    public void GetString_MaximumLengthTooLong()
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.GetString(0, 4))
            .Should().ThrowArgumentOutOfRangeException("index + maximumLength (4) must be less than or equal to Length. (3)", "maximumLength", 4);
    }

    [Test]
    public void SetString_MaximumLengthTooLong()
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetString(0, 4, ""))
            .Should().ThrowArgumentOutOfRangeException("index + maximumLength (4) must be less than or equal to Length. (3)", "maximumLength", 4);
    }

    [Test]
    public void SetString_StringTooLong()
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetString(0, 2, "ABC"))
            .Should().ThrowArgumentException("Value is longer (3) than maximumLength. (2)", "value");
    }

    [Test]
    public void SetString_StringContainsNonAsciiCharacters()
    {
        var header = new TestIOFileComponent(3);
        AssertThat.Invoking(() => header.SetString(0, 3, "A\u00D8"))
            .Should().ThrowArgumentException("Character at index 1 ('\u00D8') is not ASCII.", "value");
    }

    [Test]
    public void GetAndSetString()
    {
        var header = new TestIOFileComponent(6);
        header.GetString(1, 4).Should().Equal("");

        header.SetString(1, 4, "Test");
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual("\0Test\0"u8.ToArray());
        header.GetString(1, 3).Should().Equal("Tes");
        header.GetString(1, 4).Should().Equal("Test");
        header.GetString(1, 5).Should().Equal("Test");

        header.SetString(0, 4, "AB");
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual("AB\0\0t\0"u8.ToArray());
        header.GetString(0, 4).Should().Equal("AB");
        header.GetString(2, 4).Should().Equal("");

        header.SetString(0, 6, "");
        header.AsReadOnlySpan().ToArray().Should().SequenceEqual("\0\0\0\0\0\0"u8.ToArray());
    }

    private sealed class TestIOFileComponent : IOFileComponent
    {
        public TestIOFileComponent(int length) : base(length)
        {
        }

        public TestIOFileComponent(byte[] header) : base(header)
        {
        }

        public TestIOFileComponent(int length, Stream stream) : base(length, stream)
        {
        }

        public TestIOFileComponent(int length, IEnumerable<byte> bytes) : base(length, bytes)
        {
        }

        [Pure]
        public new byte GetByte(int index) => base.GetByte(index);

        public new void SetByte(int index, byte value) => base.SetByte(index, value);

        [Pure]
        public new TEnum GetByte<TEnum>(int index)
            where TEnum : struct, Enum =>
            base.GetByte<TEnum>(index);

        public new void SetByte<TEnum>(int index, TEnum value)
            where TEnum : struct, Enum =>
            base.SetByte(index, value);

        [Pure]
        public new ushort GetWord(int index, Endian endian = Endian.Little) => base.GetWord(index, endian);

        public new void SetWord(int index, ushort value, Endian endian = Endian.Little) => base.SetWord(index, value, endian);

        [Pure]
        public new int GetUInt24(int index, Endian endian = Endian.Little) => base.GetUInt24(index, endian);

        public new void SetUInt24(int index, int value, Endian endian = Endian.Little) => base.SetUInt24(index, value, endian);

        [Pure]
        public new int GetInt32(int index, Endian endian = Endian.Little) => base.GetInt32(index, endian);

        public new void SetInt32(int index, int value, Endian endian = Endian.Little) => base.SetInt32(index, value, endian);

        [Pure]
        public new uint GetUInt32(int index, Endian endian = Endian.Little) => base.GetUInt32(index, endian);

        public new void SetUInt32(int index, uint value, Endian endian = Endian.Little) => base.SetUInt32(index, value, endian);

        [Pure]
        public new long GetInt64(int index, Endian endian = Endian.Little) => base.GetInt64(index, endian);

        public new void SetInt64(int index, long value, Endian endian = Endian.Little) => base.SetInt64(index, value, endian);

        [Pure]
        public new ulong GetUInt64(int index, Endian endian = Endian.Little) => base.GetUInt64(index, endian);

        public new void SetUInt64(int index, ulong value, Endian endian = Endian.Little) => base.SetUInt64(index, value, endian);

        [Pure]
        public new bool GetBit(int index, int bitIndex) => base.GetBit(index, bitIndex);

        [Pure]
        public new byte GetBits(int index, int startInclusive, int endInclusive) => base.GetBits(index, startInclusive, endInclusive);

        [Pure]
        public new TEnum GetBits<TEnum>(int index, int startInclusive, int endInclusive)
            where TEnum : struct, Enum =>
            base.GetBits<TEnum>(index, startInclusive, endInclusive);

        public new void SetBit(int index, int bitIndex, bool value) => base.SetBit(index, bitIndex, value);

        public new void SetBits(int index, byte value, int startInclusive, int endInclusive) => base.SetBits(index, value, startInclusive, endInclusive);

        public new void SetBits<TEnum>(int index, TEnum value, int startInclusive, int endInclusive)
            where TEnum : struct, Enum =>
            base.SetBits(index, value, startInclusive, endInclusive);

        [Pure]
        public new string GetString(int index, int length) => base.GetString(index, length);

        public new void SetString(int index, int length, ReadOnlySpan<char> value) => base.SetString(index, length, value);

        public void SetViaAsSpan(int index, byte value) => AsSpan()[index] = value;

        public void SetViaAsSpan(int start, int index, byte value) => AsSpan(start)[index] = value;

        public void SetViaAsSpanFromEnd(Index startIndex, int index, byte value) => AsSpan(startIndex)[index] = value;
    }
}