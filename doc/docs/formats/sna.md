# SNA Snapshot Format

SNA is a simple snapshot format for the ZX Spectrum. Two variants exist: a 48K variant (49179 bytes) and a 128K variant (131103 bytes). The format stores CPU registers and RAM contents directly, with no compression.

The 48K variant stores the program counter (PC) on the stack rather than in the header, since the original format was designed to be loaded by pushing PC and executing `RETN`.

Details about the SNA format can be found at [https://sinclair.wiki.zxnet.co.uk/wiki/SNA_format](https://sinclair.wiki.zxnet.co.uk/wiki/SNA_format).

## API

| Class                                                                                 | Description                                             |
|---------------------------------------------------------------------------------------|---------------------------------------------------------|
| [`SnaFormat`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/SnaFormat/index.md)     | Singleton format for reading and writing SNA snapshots. |
| [`SnaFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/SnaFile/index.md)         | Base class for SNA snapshot files.                      |
| [`Sna48kFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/Sna48kFile/index.md)   | 48K SNA snapshot.                                       |
| [`Sna128kFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/Sna128kFile/index.md) | 128K SNA snapshot.                                      |
| [`SnaHeader`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/SnaHeader/index.md)     | Header containing CPU registers and hardware state.     |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("snapshot.sna");
SnaFile sna = SnaFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.sna");
SnaFormat.Instance.Write(sna, output);
```

The format variant (48K or 128K) is detected automatically on read.

## Accessing Registers

```c#
var regs = sna.Registers;
Console.WriteLine($"SP: 0x{regs.SP:X4}");
Console.WriteLine($"AF: 0x{regs.AF:X4}  HL: 0x{regs.HL:X4}");
```

## Creating Snapshots

[`Sna48kFile.Create`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna/Sna48kFile/index.md) takes a 64 KB memory buffer and creates a snapshot with zeroed registers:

```c#
byte[] memory = new byte[64 * 1024];
// ... populate memory ...

Sna48kFile sna = Sna48kFile.Create(memory);
```

## Conversions

SNA snapshots can be converted to Z80. See [Reading, Writing and Converting](../reading-writing-converting.md).
