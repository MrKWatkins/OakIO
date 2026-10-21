using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;

/// <summary>
/// Base class for V2 and V3 Z80 snapshot files that contain paged memory.
/// </summary>
/// <typeparam name="THeader">The type of header.</typeparam>
public abstract class Z80V2OrV3File<THeader> : Z80File<THeader>, IZ80SnapshotV2OrV3File
    where THeader : Z80V2Header
{
    private protected Z80V2OrV3File(THeader header, [InstantHandle] IEnumerable<Page> pages)
        : base(header)
    {
        Pages = [.. pages];
        if (Pages.Count == 0)
        {
            throw new ArgumentException("Value is empty.", nameof(pages));
        }
    }

    /// <summary>
    /// Gets the memory pages in the snapshot.
    /// </summary>
    public IReadOnlyList<Page> Pages { get; }

    /// <inheritdoc />
    public HardwareMode HardwareMode => Header.HardwareMode;

    /// <inheritdoc />
    public byte Port7FFD => Header.Port7FFD;

    /// <inheritdoc />
    [Pure]
    public IReadOnlyList<byte> GetBank(int bankNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bankNumber);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bankNumber, 7);
        if (HardwareMode != HardwareMode.Spectrum128)
        {
            throw new InvalidOperationException("Physical RAM banks are only available in 128K snapshots.");
        }
        var matches = Pages.Where(page => page.Header.PageNumber == bankNumber + 3).ToArray();
        if (matches.Length != 1)
        {
            throw new InvalidDataException($"Expected exactly one page for RAM bank {bankNumber}.");
        }
        return matches[0].UncompressedData;
    }

    /// <inheritdoc />
    public override RegisterSnapshot Registers => Header.Registers;

    /// <inheritdoc />
    [MustUseReturnValue]
    public sealed override bool TryLoadInto(Span<byte> memory)
    {
        if (HardwareMode == HardwareMode.Spectrum128)
        {
            GetBank(5).CopyTo(memory[0x4000..]);
            GetBank(2).CopyTo(memory[0x8000..]);
            GetBank(Port7FFD & 7).CopyTo(memory[0xC000..]);
            return true;
        }
        foreach (var page in Pages)
        {
            page.LoadInto(memory);
        }

        return true;
    }
}