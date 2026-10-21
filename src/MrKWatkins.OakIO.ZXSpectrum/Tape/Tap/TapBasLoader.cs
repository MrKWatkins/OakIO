using MrKWatkins.OakIO.ZXSpectrum.Basic;
using static MrKWatkins.OakIO.ZXSpectrum.Basic.Basic;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Builds ordinary ROM-loader BASIC, keeping the hardware paging state and BANKM shadow in sync.
/// </summary>
internal static class TapBasLoader
{
    // Paging port, lock bit and BANKM: https://worldofspectrum.org/faq/reference/128kreference.htm
    private const int BankShadow = 23388;
    private const int PagingPort = 32765;
    private const int ProgramStart = 23755;

    [Pure]
    internal static byte[] Create(IReadOnlyList<TapBasBlock> blocks, TapBasOptions options)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        ArgumentNullException.ThrowIfNull(options);
        if (blocks.Count == 0)
        {
            throw new ArgumentException("At least one load block is required.", nameof(blocks));
        }
        foreach (var block in blocks)
        {
            ArgumentNullException.ThrowIfNull(block);
        }
        var paging = options.EntryBank != null || blocks.Any(block => block.Bank != null);
        Validate(blocks, options, paging);

        using var stream = new MemoryStream();
        var writer = new BasicWriter(stream);
        var line = 10;
        Write(CLEAR, options.ClearAddress);
        Write(POKE, 23610, ',', 255);
        if (paging)
        {
            // Test bit 5 without a bitwise AND (BASIC's AND is logical). STOP before loading if paging is locked.
            Write(IF, INT, '(', PEEK, BankShadow, '/', 32, ')', '-', 2, '*', INT, '(', PEEK, BankShadow, '/', 64, ')', THEN, STOP);
        }
        foreach (var block in blocks)
        {
            if (block.Bank is { } bank)
            {
                Select(bank);
            }
            Write(LOAD, "", CODE);
        }
        if (paging)
        {
            Select(options.EntryBank ?? 0);
        }
        if (options.EntryPoint is { } entry)
        {
            Write(RANDOMIZE, USR, entry);
        }
        var data = stream.ToArray();
        if (ProgramStart + data.Length > options.ClearAddress)
        {
            throw new ArgumentException("The BASIC loader does not fit below ClearAddress.", nameof(options));
        }
        return data;

        void Select(byte bank)
        {
            // Preserve ROM, display, lock and unused high bits; replace only the three bank-select bits.
            Write(POKE, BankShadow, ',', 8, '*', INT, '(', PEEK, BankShadow, '/', 8, ')', '+', (int)bank);
            Write(OUT, PagingPort, ',', PEEK, BankShadow);
        }

        void Write(params object[] symbols)
        {
            if (line > 9999)
            {
                throw new ArgumentException("The BASIC loader exceeds the maximum line number.", nameof(blocks));
            }
            writer.WriteLine(line, symbols);
            line += 10;
        }
    }

    private static void Validate(IReadOnlyList<TapBasBlock> blocks, TapBasOptions options, bool paging)
    {
        if (options.ClearAddress < ProgramStart || paging && options.ClearAddress >= 0xC000)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "ClearAddress must leave BASIC workspace in fixed RAM when paging.");
        }
        if (options.EntryBank is > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "EntryBank must be in the range 0–7.");
        }
        foreach (var block in blocks)
        {
            if (block.Bank is > 7 || block.Data.Length is 0 or > 65533 || block.Location < 0x4000 || block.Data.Length > 65536 - block.Location)
            {
                throw new ArgumentException("Load blocks require nonempty RAM ranges, valid bank numbers and TAP-sized payloads.", nameof(blocks));
            }
            if (block is { Bank: not null, Location: < 0xC000 } || paging && block.Bank == null && block.Location + block.Data.Length > 0xC000)
            {
                throw new ArgumentException("Banked blocks must fit at $C000–$FFFF; unbanked blocks must stay below $C000 when paging.", nameof(blocks));
            }
            // Banks 2 and 5 alias fixed RAM, including the loader, BASIC variables and its stack.
            var fixedAddress = block.Bank switch
            {
                null => (int?)block.Location,
                2 => block.Location - 0x4000,
                5 => block.Location - 0x8000,
                _ => null
            };
            if (fixedAddress is { } start && start <= options.ClearAddress && start + block.Data.Length > 0x5B00)
            {
                throw new ArgumentException("A load block overlaps BASIC's reserved workspace or stack.", nameof(blocks));
            }
        }
    }
}