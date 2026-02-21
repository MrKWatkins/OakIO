namespace MrKWatkins.OakIO.ZXSpectrum.Nex;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class NexHeader : Header
{
    internal const int Size = 512;

    public NexHeader()
        : this(new byte[Size])
    {
        SetString(0, 4, "Next");
        SetString(4, 4, "V1.2");
    }

    internal NexHeader(byte[] data)
        : base(data)
    {
    }

    public string Magic => GetString(0, 4);

    public string VersionString
    {
        get => GetString(4, 4);
        set => SetString(4, 4, value);
    }

    public NexVersion Version => VersionString switch
    {
        "V1.3" => NexVersion.V13,
        _ => NexVersion.V12
    };

    public NexRamRequired RamRequired
    {
        get => GetByte<NexRamRequired>(8);
        set => SetByte(8, value);
    }

    public byte NumBanksToLoad
    {
        get => GetByte(9);
        set => SetByte(9, value);
    }

    public bool HasNoPaletteBlock
    {
        get => GetBit(10, 7);
        set => SetBit(10, 7, value);
    }

    public bool LoadScreenFlags2
    {
        get => GetBit(10, 6);
        set => SetBit(10, 6, value);
    }

    public bool HasHiColourScreen
    {
        get => GetBit(10, 4);
        set => SetBit(10, 4, value);
    }

    public bool HasHiResScreen
    {
        get => GetBit(10, 3);
        set => SetBit(10, 3, value);
    }

    public bool HasLoResScreen
    {
        get => GetBit(10, 2);
        set => SetBit(10, 2, value);
    }

    public bool HasUlaScreen
    {
        get => GetBit(10, 1);
        set => SetBit(10, 1, value);
    }

    public bool HasLayer2Screen
    {
        get => GetBit(10, 0);
        set => SetBit(10, 0, value);
    }

    internal byte LoadScreensByte => GetByte(10);

    public ZXColour BorderColour
    {
        get => GetByte<ZXColour>(11);
        set => SetByte(11, value);
    }

    public ushort SP
    {
        get => GetWord(12);
        set => SetWord(12, value);
    }

    public ushort PC
    {
        get => GetWord(14);
        set => SetWord(14, value);
    }

    public ushort NumExtraFiles
    {
        get => GetWord(16);
        set => SetWord(16, value);
    }

    public bool IsBankIncluded(int bank)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bank);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bank, 112);
        return GetByte(18 + bank) != 0;
    }

    public void SetBankIncluded(int bank, bool included)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bank);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bank, 112);
        SetByte(18 + bank, included ? (byte)1 : (byte)0);
    }

    public byte LoadingBar
    {
        get => GetByte(130);
        set => SetByte(130, value);
    }

    public byte LoadingBarColour
    {
        get => GetByte(131);
        set => SetByte(131, value);
    }

    public byte LoadingDelay
    {
        get => GetByte(132);
        set => SetByte(132, value);
    }

    public byte StartDelay
    {
        get => GetByte(133);
        set => SetByte(133, value);
    }

    public bool PreserveNextRegisters
    {
        get => GetByte(134) != 0;
        set => SetByte(134, value ? (byte)1 : (byte)0);
    }

    public byte CoreVersionMajor
    {
        get => GetByte(135);
        set => SetByte(135, value);
    }

    public byte CoreVersionMinor
    {
        get => GetByte(136);
        set => SetByte(136, value);
    }

    public byte CoreVersionSubMinor
    {
        get => GetByte(137);
        set => SetByte(137, value);
    }

    public byte HiResColour
    {
        get => GetByte(138);
        set => SetByte(138, value);
    }

    public byte EntryBank
    {
        get => GetByte(139);
        set => SetByte(139, value);
    }

    public ushort FileHandleAddress
    {
        get => GetWord(140);
        set => SetWord(140, value);
    }

    public bool ExpansionBusEnable
    {
        get => GetByte(142) != 0;
        set => SetByte(142, value ? (byte)1 : (byte)0);
    }

    public bool HasChecksum
    {
        get => GetByte(143) != 0;
        set => SetByte(143, value ? (byte)1 : (byte)0);
    }

    public uint BanksOffset
    {
        get => GetUInt32(144);
        set => SetUInt32(144, value);
    }

    public ushort CliBufferAddress
    {
        get => GetWord(148);
        set => SetWord(148, value);
    }

    public ushort CliBufferSize
    {
        get => GetWord(150);
        set => SetWord(150, value);
    }

    public NexLoadScreenMode LoadScreens2
    {
        get => GetByte<NexLoadScreenMode>(152);
        set => SetByte(152, value);
    }

    public bool HasCopperCode
    {
        get => GetByte(153) != 0;
        set => SetByte(153, value ? (byte)1 : (byte)0);
    }

    public byte TileScreenConfigReg6B
    {
        get => GetByte(154);
        set => SetByte(154, value);
    }

    public byte TileScreenConfigReg6C
    {
        get => GetByte(155);
        set => SetByte(155, value);
    }

    public byte TileScreenConfigReg6E
    {
        get => GetByte(156);
        set => SetByte(156, value);
    }

    public byte TileScreenConfigReg6F
    {
        get => GetByte(157);
        set => SetByte(157, value);
    }

    public byte BigL2BarPosY
    {
        get => GetByte(158);
        set => SetByte(158, value);
    }

    public uint Crc32C
    {
        get => GetUInt32(508);
        set => SetUInt32(508, value);
    }

    internal static readonly int[] BankOrder =
    [
        5, 2, 0, 1, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
        32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
        48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
        64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
        80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95,
        96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111
    ];
}