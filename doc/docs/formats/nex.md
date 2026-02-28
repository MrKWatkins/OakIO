# NEX Snapshot Format

NEX is a snapshot format for the ZX Spectrum Next. In addition to the standard CPU registers and RAM banks, it supports Next-specific features including multiple screen types (Layer 2, ULA, LoRes, HiRes, HiColour), copper code, and a palette. NEX does not store shadow register data.

Details about the NEX format can be found at [https://wiki.specnext.dev/NEX_file_format](https://wiki.specnext.dev/NEX_file_format).

## API

| Class                                                                             | Description                                                      |
|-----------------------------------------------------------------------------------|------------------------------------------------------------------|
| [`NexFormat`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex/NexFormat/index.md) | Singleton format for reading and writing NEX snapshots.          |
| [`NexFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex/NexFile/index.md)     | Represents a NEX snapshot file.                                  |
| [`NexHeader`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex/NexHeader/index.md) | 512-byte header containing registers and hardware configuration. |
| [`NexBank`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex/NexBank/index.md)     | A 16 KB RAM bank.                                                |
| [`NexScreen`](../API/MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex/NexScreen/index.md) | An attached screen buffer.                                       |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("snapshot.nex");
NexFile nex = NexFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.nex");
NexFormat.Instance.Write(nex, output);
```

## Accessing State

```c#
// CPU registers
var regs = nex.Registers;
Console.WriteLine($"PC: 0x{regs.PC:X4}  SP: 0x{regs.SP:X4}");

// Header fields
Console.WriteLine($"Version: {nex.Header.VersionString}");
Console.WriteLine($"RAM Required: {nex.Header.RamRequired}");
Console.WriteLine($"Entry Bank: {nex.Header.EntryBank}");

// Banks
foreach (NexBank bank in nex.Banks)
    Console.WriteLine($"Bank {bank.BankNumber}: {bank.Data.Length} bytes");

// Screens
foreach (NexScreen screen in nex.Screens)
    Console.WriteLine($"Screen: {screen.Type}");
```
