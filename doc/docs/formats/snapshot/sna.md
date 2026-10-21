# SNA

SNA is a simple snapshot format for the ZX Spectrum. Two variants exist: a 48K variant (49179 bytes) and a 128K variant (131103 or 147487 bytes). The larger 128K form stores a duplicate bank when bank 2 or 5 is selected at `$C000`. The format stores CPU registers and RAM contents directly, with no compression.

The 48K variant stores the program counter (PC) on the stack rather than in the header, since the original format was designed to be loaded by pushing PC and executing `RETN`.

Details about the SNA format can be found at [https://sinclair.wiki.zxnet.co.uk/wiki/SNA_format](https://sinclair.wiki.zxnet.co.uk/wiki/SNA_format).

## API

| Class                                                                                    | Description                                             |
|------------------------------------------------------------------------------------------|---------------------------------------------------------|
| [`SnaFormat`](../../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/SnaFormat/index.md)     | Singleton format for reading and writing SNA snapshots. |
| [`SnaFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/SnaFile/index.md)         | Base class for SNA snapshot files.                      |
| [`Sna48kFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/Sna48kFile/index.md)   | 48K SNA snapshot.                                       |
| [`Sna128kFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/Sna128kFile/index.md) | 128K SNA snapshot.                                      |
| [`SnaHeader`](../../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/SnaHeader/index.md)     | Header containing CPU registers and hardware state.     |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("snapshot.sna");
SnaFile sna = SnaFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.sna");
sna.Write(output);
```

The format variant (48K or 128K) is detected automatically on read.

## Accessing Registers

```c#
var regs = sna.Registers;
Console.WriteLine($"SP: 0x{regs.SP:X4}");
Console.WriteLine($"AF: 0x{regs.AF:X4}  HL: 0x{regs.HL:X4}");
```

## Creating Snapshots

[`Sna48kFile.Create`](../../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/Sna48kFile/index.md) takes a 64 KB memory buffer and creates a snapshot with zeroed registers:

```c#
byte[] memory = new byte[64 * 1024];
// ... populate memory ...

Sna48kFile sna = Sna48kFile.Create(memory);
```

[`Sna128kFile.Create`](../../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/Sna128kFile/index.md) takes exactly eight physical RAM banks, numbered 0–7, each containing 16384 bytes. It copies the data and starts with zeroed registers:

```c#
ReadOnlyMemory<byte>[] banks = Enumerable.Range(0, 8)
    .Select(_ => (ReadOnlyMemory<byte>)new byte[16384]).ToArray();
// ... populate each physical bank ...
Sna128kFile sna128 = Sna128kFile.Create(banks, port7FFD: 0x10);
sna128.Registers.PC = 0x8000;
sna128.Registers.SP = 0xBFFE;
```

The default `$7FFD` value is `$10`: 48K ROM, RAM bank 0, normal screen and unlocked paging. The factory does not initialize BASIC system variables, including the `BANKM` shadow at `$5B5C`. The caller must provide any workspace its program requires. Unlike 48K SNA, the 128K format stores PC separately and does not alter the stack to save it. `GetBank(number)` accesses physical RAM; `TryLoadInto` copies the visible banks 5, 2 and the selected bank into a 64 KB buffer, leaving the ROM area unchanged.

The bank ordering, paging footer and duplicate-bank form follow the [SNA format reference](https://worldofspectrum.net/zx-modules/fileformats/snaformat.html).

## Conversions

SNA snapshots can be converted to Z80. A 128K conversion preserves all eight RAM banks, CPU registers and `$7FFD` state rather than converting only the visible 48 KB. Conversion rejects a paged-in TR-DOS ROM, which the supported Z80 format cannot represent. See [Reading, Writing and Converting](../../reading-writing-converting.md).
