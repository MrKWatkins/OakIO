using System.Globalization;
using System.Text;
using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public sealed class TapBasLoaderTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Create(bool entry)
    {
        var data = TapBasLoader.Create([new TapBasBlock(0x8000, new byte[1]), new TapBasBlock(0x9000, new byte[1])], new TapBasOptions
        {
            ClearAddress = 0x6FFF,
            EntryPoint = entry ? 0x8123 : null
        });
        var expected = new List<string>
        {
            "10 CLEAR 28671", "20 POKE 23610,255", "30 LOAD \"\" CODE", "40 LOAD \"\" CODE"
        };
        if (entry)
        {
            expected.Add("50 RANDOMIZE USR 33059");
        }
        Listing(data).Should().SequenceEqual(expected);
    }

    [TestCase((byte)0)]
    [TestCase((byte)1)]
    [TestCase((byte)2)]
    [TestCase((byte)3)]
    [TestCase((byte)4)]
    [TestCase((byte)5)]
    [TestCase((byte)6)]
    [TestCase((byte)7)]
    public void Create_Banked(byte bank)
    {
        var data = TapBasLoader.Create([new TapBasBlock(0xD000, new byte[1])
        {
            Bank = bank
        }], new TapBasOptions());
        Listing(data).Should().SequenceEqual(
            "10 CLEAR 32767",
            "20 POKE 23610,255",
            "30 IF INT (PEEK 23388/32)-2*INT (PEEK 23388/64) THEN STOP",
            $"40 POKE 23388,8*INT (PEEK 23388/8)+{bank}",
            "50 OUT 32765,PEEK 23388",
            "60 LOAD \"\" CODE",
            "70 POKE 23388,8*INT (PEEK 23388/8)+0",
            "80 OUT 32765,PEEK 23388");
    }

    [Test]
    public void Create_MixedBlocks()
    {
        var data = TapBasLoader.Create([
            new TapBasBlock(0x8000, new byte[1]),
            new TapBasBlock(0xC000, new byte[1])
            {
                Bank = 3
            },
            new TapBasBlock(0x9000, new byte[1]),
            new TapBasBlock(0xD000, new byte[1])
            {
                Bank = 3
            }
        ], new TapBasOptions
        {
            EntryBank = 6,
            EntryPoint = 0x8000
        });
        Listing(data).Should().SequenceEqual(
            "10 CLEAR 32767",
            "20 POKE 23610,255",
            "30 IF INT (PEEK 23388/32)-2*INT (PEEK 23388/64) THEN STOP",
            "40 LOAD \"\" CODE",
            "50 POKE 23388,8*INT (PEEK 23388/8)+3",
            "60 OUT 32765,PEEK 23388",
            "70 LOAD \"\" CODE",
            "80 LOAD \"\" CODE",
            "90 POKE 23388,8*INT (PEEK 23388/8)+3",
            "100 OUT 32765,PEEK 23388",
            "110 LOAD \"\" CODE",
            "120 POKE 23388,8*INT (PEEK 23388/8)+6",
            "130 OUT 32765,PEEK 23388",
            "140 RANDOMIZE USR 32768");
    }

    [Test]
    public void Create_EntryBankOnly()
    {
        var lines = Listing(TapBasLoader.Create([new TapBasBlock(0x8000, new byte[1])], new TapBasOptions
        {
            EntryBank = 7
        }));
        lines.Should().SequenceEqual(
            "10 CLEAR 32767", "20 POKE 23610,255",
            "30 IF INT (PEEK 23388/32)-2*INT (PEEK 23388/64) THEN STOP",
            "40 LOAD \"\" CODE", "50 POKE 23388,8*INT (PEEK 23388/8)+7", "60 OUT 32765,PEEK 23388");
    }

    [TestCase(0x4000, 6912, null)]
    [TestCase(0x8000, 16384, null)]
    [TestCase(0xC000, 16384, 0)]
    [TestCase(0xFFFF, 1, 7)]
    [TestCase(0xC000, 6912, 5)]
    [TestCase(0xC000, 16384, 2)]
    public void Create_Boundaries(int location, int length, int? bank)
    {
        var data = TapBasLoader.Create([new TapBasBlock((ushort)location, new byte[length])
        {
            Bank = bank == null ? null : (byte)bank
        }], new TapBasOptions());
        Listing(data).Count(line => line.Contains("LOAD", StringComparison.Ordinal)).Should().Equal(1);
    }

    [TestCase(0x3FFF, 1, null)]
    [TestCase(0x8000, 0, null)]
    [TestCase(0x4000, 65534, null)]
    [TestCase(0xFFFF, 2, null)]
    [TestCase(0x8000, 1, 0)]
    [TestCase(0xC000, 16385, 0)]
    [TestCase(0xC000, 1, 8)]
    [TestCase(0xC000, 1, 255)]
    [TestCase(0x4000, 6913, null)]
    [TestCase(0x5B00, 1, null)]
    [TestCase(0x7FFF, 1, null)]
    [TestCase(0xC000, 6913, 5)]
    public void Create_InvalidBlock(int location, int length, int? bank) =>
        AssertThat.Invoking(() => TapBasLoader.Create([new TapBasBlock((ushort)location, new byte[length])
        {
            Bank = bank == null ? null : (byte)bank
        }], new TapBasOptions())).Should().Throw<ArgumentException>();

    [TestCase(0xBFFF, 2)]
    [TestCase(0xC000, 1)]
    public void Create_UnbankedWindowCrossing(int location, int length) =>
        AssertThat.Invoking(() => TapBasLoader.Create([new TapBasBlock((ushort)location, new byte[length])], new TapBasOptions
        {
            EntryBank = 0
        })).Should().Throw<ArgumentException>();

    [TestCase(0)]
    [TestCase(23754)]
    [TestCase(0xC000)]
    [TestCase(0xFFFF)]
    public void Create_InvalidClearAddress(int address) =>
        AssertThat.Invoking(() => TapBasLoader.Create([new TapBasBlock(0xC000, new byte[1])
        {
            Bank = 0
        }], new TapBasOptions
        {
            ClearAddress = (ushort)address
        })).Should().Throw<ArgumentOutOfRangeException>();

    [TestCase((byte)8)]
    [TestCase((byte)255)]
    public void Create_InvalidEntryBank(byte bank) =>
        AssertThat.Invoking(() => TapBasLoader.Create([new TapBasBlock(0x8000, new byte[1])], new TapBasOptions
        {
            EntryBank = bank
        })).Should().Throw<ArgumentOutOfRangeException>();

    [Test]
    public void Create_AliasedWorkspace() =>
        AssertThat.Invoking(() => TapBasLoader.Create([new TapBasBlock(0xC000, new byte[1])
        {
            Bank = 2
        }], new TapBasOptions
        {
            ClearAddress = 0x8FFF
        })).Should().Throw<ArgumentException>();

    [Test]
    public void Create_Empty() => AssertThat.Invoking(() => TapBasLoader.Create([], new TapBasOptions())).Should().Throw<ArgumentException>();

    [Test]
    public void Create_NullBlocks() => AssertThat.Invoking(() => TapBasLoader.Create(null!, new TapBasOptions())).Should().Throw<ArgumentNullException>();

    [Test]
    public void Create_NullBlock() => AssertThat.Invoking(() => TapBasLoader.Create([null!], new TapBasOptions())).Should().Throw<ArgumentNullException>();

    [Test]
    public void Create_NullOptions() => AssertThat.Invoking(() => TapBasLoader.Create([new TapBasBlock(0x8000, new byte[1])], null!)).Should().Throw<ArgumentNullException>();

    [Test]
    public void Create_LoaderTooLarge() =>
        AssertThat.Invoking(() => TapBasLoader.Create([new TapBasBlock(0x8000, new byte[1])], new TapBasOptions
        {
            ClearAddress = 23755
        })).Should().Throw<ArgumentException>();

    [Test]
    public void Create_TooManyLines() =>
        AssertThat.Invoking(() => TapBasLoader.Create(Enumerable.Repeat(new TapBasBlock(0x4000, new byte[1]), 1000).ToArray(), new TapBasOptions()))
            .Should().Throw<ArgumentException>();

    [Pure]
    private static IReadOnlyList<string> Listing(byte[] data)
    {
        var lines = new List<string>();
        var offset = 0;
        while (offset < data.Length)
        {
            var number = data.GetUInt16(offset, Endian.Big);
            var length = data.GetUInt16(offset + 2);
            var end = offset + 4 + length;
            data[end - 1].Should().Equal((byte)13);
            var text = new StringBuilder();
            for (var index = offset + 4; index < end - 1; index++)
            {
                if (data[index] == 14)
                {
                    // Independently check the binary value as well as its displayed digits.
                    var digits = text.ToString();
                    var start = digits.Length;
                    while (start > 0 && char.IsAsciiDigit(digits[start - 1]))
                    {
                        start--;
                    }
                    var value = ushort.Parse(digits.AsSpan(start), CultureInfo.InvariantCulture);
                    data.AsSpan(index + 1, 5).ToArray().Should().SequenceEqual(0, 0, (byte)value, (byte)(value >> 8), 0);
                    index += 5;
                    continue;
                }
                text.Append(data[index] switch
                {
                    0xFD => "CLEAR ",
                    0xF4 => "POKE ",
                    0xEF => "LOAD ",
                    0xAF => " CODE",
                    0xF9 => "RANDOMIZE ",
                    0xC0 => "USR ",
                    0xBA => "INT ",
                    0xBE => "PEEK ",
                    0xDF => "OUT ",
                    0xFA => "IF ",
                    0xCB => " THEN ",
                    0xE2 => "STOP",
                    var character => ((char)character).ToString()
                });
            }
            lines.Add($"{number} {text}");
            offset = end;
        }
        offset.Should().Equal(data.Length);
        return lines;
    }
}