namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;

/// <summary>
/// The header of a NEX snapshot file.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class NexHeader : Header
{
    internal const int Size = 512;

    /// <summary>
    /// Initialises a new instance of the <see cref="NexHeader" /> class with default values.
    /// </summary>
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

    /// <summary>
    /// Gets the magic identifier string.
    /// </summary>
    public string Magic => GetString(0, 4);

    /// <summary>
    /// Gets or sets the version string.
    /// </summary>
    public string VersionString
    {
        get => GetString(4, 4);
        set => SetString(4, 4, value);
    }

    /// <summary>
    /// Gets the parsed NEX version.
    /// </summary>
    public NexVersion Version => VersionString switch
    {
        "V1.3" => NexVersion.V13,
        _ => NexVersion.V12
    };

    /// <summary>
    /// Gets or sets the amount of RAM required.
    /// </summary>
    public NexRamRequired RamRequired
    {
        get => GetByte<NexRamRequired>(8);
        set => SetByte(8, value);
    }

    /// <summary>
    /// Gets or sets the number of banks to load.
    /// </summary>
    public byte NumBanksToLoad
    {
        get => GetByte(9);
        set => SetByte(9, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file has no palette block.
    /// </summary>
    public bool HasNoPaletteBlock
    {
        get => GetBit(10, 7);
        set => SetBit(10, 7, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the load screen flags 2 field is used.
    /// </summary>
    public bool LoadScreenFlags2
    {
        get => GetBit(10, 6);
        set => SetBit(10, 6, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file has a hi-colour loading screen.
    /// </summary>
    public bool HasHiColourScreen
    {
        get => GetBit(10, 4);
        set => SetBit(10, 4, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file has a hi-res loading screen.
    /// </summary>
    public bool HasHiResScreen
    {
        get => GetBit(10, 3);
        set => SetBit(10, 3, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file has a lo-res loading screen.
    /// </summary>
    public bool HasLoResScreen
    {
        get => GetBit(10, 2);
        set => SetBit(10, 2, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file has a ULA loading screen.
    /// </summary>
    public bool HasUlaScreen
    {
        get => GetBit(10, 1);
        set => SetBit(10, 1, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file has a Layer 2 loading screen.
    /// </summary>
    public bool HasLayer2Screen
    {
        get => GetBit(10, 0);
        set => SetBit(10, 0, value);
    }

    internal byte LoadScreensByte => GetByte(10);

    /// <summary>
    /// Gets or sets the border colour.
    /// </summary>
    public ZXColour BorderColour
    {
        get => GetByte<ZXColour>(11);
        set => SetByte(11, value);
    }

    /// <summary>
    /// Gets or sets the initial stack pointer value.
    /// </summary>
    public ushort SP
    {
        get => GetWord(12);
        set => SetWord(12, value);
    }

    /// <summary>
    /// Gets or sets the initial program counter value.
    /// </summary>
    public ushort PC
    {
        get => GetWord(14);
        set => SetWord(14, value);
    }

    /// <summary>
    /// Gets or sets the number of extra files.
    /// </summary>
    public ushort NumExtraFiles
    {
        get => GetWord(16);
        set => SetWord(16, value);
    }

    /// <summary>
    /// Gets a value indicating whether the specified bank is included in the file.
    /// </summary>
    /// <param name="bank">The bank number, from 0 to 111.</param>
    /// <returns><c>true</c> if the bank is included; otherwise, <c>false</c>.</returns>
    public bool IsBankIncluded(int bank)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bank);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bank, 112);
        return GetByte(18 + bank) != 0;
    }

    /// <summary>
    /// Sets whether the specified bank is included in the file.
    /// </summary>
    /// <param name="bank">The bank number, from 0 to 111.</param>
    /// <param name="included"><c>true</c> to include the bank; <c>false</c> to exclude it.</param>
    public void SetBankIncluded(int bank, bool included)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bank);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bank, 112);
        SetByte(18 + bank, included ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Gets or sets the loading bar style.
    /// </summary>
    public byte LoadingBar
    {
        get => GetByte(130);
        set => SetByte(130, value);
    }

    /// <summary>
    /// Gets or sets the loading bar colour.
    /// </summary>
    public byte LoadingBarColour
    {
        get => GetByte(131);
        set => SetByte(131, value);
    }

    /// <summary>
    /// Gets or sets the loading delay in frames.
    /// </summary>
    public byte LoadingDelay
    {
        get => GetByte(132);
        set => SetByte(132, value);
    }

    /// <summary>
    /// Gets or sets the start delay in frames.
    /// </summary>
    public byte StartDelay
    {
        get => GetByte(133);
        set => SetByte(133, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether Next registers should be preserved on load.
    /// </summary>
    public bool PreserveNextRegisters
    {
        get => GetByte(134) != 0;
        set => SetByte(134, value ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Gets or sets the required core version major number.
    /// </summary>
    public byte CoreVersionMajor
    {
        get => GetByte(135);
        set => SetByte(135, value);
    }

    /// <summary>
    /// Gets or sets the required core version minor number.
    /// </summary>
    public byte CoreVersionMinor
    {
        get => GetByte(136);
        set => SetByte(136, value);
    }

    /// <summary>
    /// Gets or sets the required core version sub-minor number.
    /// </summary>
    public byte CoreVersionSubMinor
    {
        get => GetByte(137);
        set => SetByte(137, value);
    }

    /// <summary>
    /// Gets or sets the hi-res screen colour.
    /// </summary>
    public byte HiResColour
    {
        get => GetByte(138);
        set => SetByte(138, value);
    }

    /// <summary>
    /// Gets or sets the bank paged in at entry.
    /// </summary>
    public byte EntryBank
    {
        get => GetByte(139);
        set => SetByte(139, value);
    }

    /// <summary>
    /// Gets or sets the file handle address.
    /// </summary>
    public ushort FileHandleAddress
    {
        get => GetWord(140);
        set => SetWord(140, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the expansion bus is enabled.
    /// </summary>
    public bool ExpansionBusEnable
    {
        get => GetByte(142) != 0;
        set => SetByte(142, value ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file has a CRC-32C checksum.
    /// </summary>
    public bool HasChecksum
    {
        get => GetByte(143) != 0;
        set => SetByte(143, value ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Gets or sets the offset to the banks data.
    /// </summary>
    public uint BanksOffset
    {
        get => GetUInt32(144);
        set => SetUInt32(144, value);
    }

    /// <summary>
    /// Gets or sets the CLI buffer address.
    /// </summary>
    public ushort CliBufferAddress
    {
        get => GetWord(148);
        set => SetWord(148, value);
    }

    /// <summary>
    /// Gets or sets the CLI buffer size.
    /// </summary>
    public ushort CliBufferSize
    {
        get => GetWord(150);
        set => SetWord(150, value);
    }

    /// <summary>
    /// Gets or sets the secondary load screen mode.
    /// </summary>
    public NexLoadScreenMode LoadScreens2
    {
        get => GetByte<NexLoadScreenMode>(152);
        set => SetByte(152, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file contains copper code.
    /// </summary>
    public bool HasCopperCode
    {
        get => GetByte(153) != 0;
        set => SetByte(153, value ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Gets or sets the tilemap screen configuration register 0x6B value.
    /// </summary>
    public byte TileScreenConfigReg6B
    {
        get => GetByte(154);
        set => SetByte(154, value);
    }

    /// <summary>
    /// Gets or sets the tilemap screen configuration register 0x6C value.
    /// </summary>
    public byte TileScreenConfigReg6C
    {
        get => GetByte(155);
        set => SetByte(155, value);
    }

    /// <summary>
    /// Gets or sets the tilemap screen configuration register 0x6E value.
    /// </summary>
    public byte TileScreenConfigReg6E
    {
        get => GetByte(156);
        set => SetByte(156, value);
    }

    /// <summary>
    /// Gets or sets the tilemap screen configuration register 0x6F value.
    /// </summary>
    public byte TileScreenConfigReg6F
    {
        get => GetByte(157);
        set => SetByte(157, value);
    }

    /// <summary>
    /// Gets or sets the Y position of the big Layer 2 loading bar.
    /// </summary>
    public byte BigL2BarPosY
    {
        get => GetByte(158);
        set => SetByte(158, value);
    }

    /// <summary>
    /// Gets or sets the CRC-32C checksum.
    /// </summary>
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