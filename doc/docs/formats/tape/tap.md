# TAP

TAP is a simple tape format for the ZX Spectrum. A TAP file is a flat sequence of blocks, each consisting of a flag byte, raw data bytes, and a checksum byte. TAP carries no metadata about the recording itself — it is purely the data that would have been read from or written to tape.

Details about the TAP format can be found at [https://sinclair.wiki.zxnet.co.uk/wiki/TAP_format](https://sinclair.wiki.zxnet.co.uk/wiki/TAP_format).

## API

| Class                                                                                | Description                                         |
|--------------------------------------------------------------------------------------|-----------------------------------------------------|
| [`TapFormat`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapFormat/index.md)     | Singleton format for reading and writing TAP files. |
| [`TapFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapFile/index.md)         | Represents a TAP file as a list of blocks.          |
| [`TapBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapBlock/index.md)       | Base class for all TAP blocks.                      |
| [`HeaderBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/HeaderBlock/index.md) | A header block describing the file that follows.    |
| [`DataBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/DataBlock/index.md)     | A data block containing the file contents.          |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("tape.tap");
TapFile tap = TapFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.tap");
tap.Write(output);
```

## Blocks

A [`TapFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapFile/index.md) contains a list of [`TapBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapBlock/index.md) objects. Each block is either a [`HeaderBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/HeaderBlock/index.md) or a [`DataBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/DataBlock/index.md):

```c#
foreach (TapBlock block in tap.Blocks)
{
    switch (block)
    {
        case HeaderBlock header:
            Console.WriteLine($"Header: {header.HeaderType} — {header.Filename}");
            break;
        case DataBlock data:
            Console.WriteLine($"Data: {data.Header.BlockLength} bytes");
            break;
    }
}
```

[`HeaderBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/HeaderBlock/index.md) carries a [`TapHeaderType`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapHeaderType/index.md) (Program, NumberArray, CharacterArray, or Code), a filename, and format-specific fields such as `DataBlockLength` and `Location`.

## Creating TAP Files

Factory methods on [`HeaderBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/HeaderBlock/index.md) and [`DataBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/DataBlock/index.md) create individual blocks, and factory methods on [`TapFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapFile/index.md) create a file with correctly paired header/data block sequences:

```c#
// A code block at location 0x8000
TapFile code = TapFile.CreateCode("game", 0x8000, myBytes);

// A loader that auto-runs and loads code blocks
TapFile loader = TapFile.CreateLoader("loader", (0x8000, myBytes));
```

## TapBAS Loaders

The existing `TapFile.CreateTapBas(filename, location, data, entryPoint)` overload creates a BASIC loader for one code block. The multi-block overload accepts an ordered list of [`TapBasBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapBasBlock/index.md) objects and optional [`TapBasOptions`](../../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapBasOptions/index.md):

```c#
TapFile file = TapFile.CreateTapBas("game",
[
    new TapBasBlock(0x8000, residentCode),
    new TapBasBlock(0xC000, forestData)
    {
        Bank = 0
    },
    new TapBasBlock(0xC000, castleData)
    {
        Bank = 1
    }
], new TapBasOptions
{
    ClearAddress = 0x7FFF,
    EntryPoint = 0x8000,
    EntryBank = 0
});
```

Blocks appear on tape in the supplied order. Each has an ordinary code header and data block; `Bank` controls the BASIC loader, not the TAP header. File creation copies the supplied data. Repeated banks and partial-bank loads are supported.

With no `Bank` values or `EntryBank`, the loader performs ordinary loads without paging. Banked blocks select a physical 128K RAM bank (0–7) before loading at a CPU address in `$C000`–`$FFFF`. Blocks without a bank must remain below `$C000` when paging is used. The loader updates the `BANKM` system variable at `$5B5C`, then writes port `$7FFD`, preserving the ROM, screen and other control bits. After loading, it selects `EntryBank`, defaulting to bank 0 for a banked loader. An optional `EntryPoint` adds `RANDOMIZE USR`; without it, execution returns to BASIC.

Banked loaders require a 128K machine with paging unlocked and an accurate `BANKM` shadow. If the shadow indicates that paging is locked, the loader executes `STOP` before loading code. This is not a test for the presence of 128K hardware. See the [128K memory and paging reference](https://worldofspectrum.org/faq/reference/128kreference.htm).

`ClearAddress` defaults to `$7FFF` and sets the highest address BASIC may use. It must remain below `$C000` for banked loads. The factory rejects empty blocks, invalid banks, address overflow, blocks crossing the paging window, and writes into BASIC's reserved workspace. This includes bank 2 and bank 5 accesses that alias fixed RAM. A screen block ending at `$5B00` is allowed. Loader-size validation assumes the standard BASIC program address `$5CCB`; callers must leave enough additional space for BASIC's runtime and any extensions.

`CLEAR` protects the loaded program but does not set its machine-code stack pointer. The entry routine must establish its own stack if required, in memory that remains mapped while the application runs.

## Conversions

TAP files can be converted to TZX, PZX, and WAV. See [Reading, Writing and Converting](../../reading-writing-converting.md).
