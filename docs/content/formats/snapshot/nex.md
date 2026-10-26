# NEX

NEX is provided by the `MrKWatkins.OakIO.ZXSpectrumNext` package in the
`MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex` namespace. `NexFile` continues to inherit from
`ZXSpectrumSnapshotFile` in the ZX Spectrum package.

When upgrading from an earlier version, add the Next package and update NEX namespace imports.
`ZXSpectrumFileFormat.AllFormats` and its `Load` methods no longer discover NEX. Use `NexFormat.Instance`
directly, or pass `ZXSpectrumNextFileFormats.AllFormats` to `IOFileFormat.Load` or `LoadAsync`.

NEX is a snapshot format for the ZX Spectrum Next. Alongside the standard CPU registers and RAM banks, it supports Next-specific features including multiple screen types (Layer 2, ULA, LoRes, HiRes, HiColour), copper code, and a palette. NEX does not store shadow register data.

Details about the NEX format can be found at [https://wiki.specnext.dev/NEX_file_format](https://wiki.specnext.dev/NEX_file_format).

## API

| Class                                                                                | Description                                                      |
|--------------------------------------------------------------------------------------|------------------------------------------------------------------|
| [`NexFormat`](../../api/MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex/NexFormat/index.md) | Singleton format for reading and writing NEX snapshots.          |
| [`NexFile`](../../api/MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex/NexFile/index.md)     | Represents a NEX snapshot file.                                  |
| [`NexHeader`](../../api/MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex/NexHeader/index.md) | 512-byte header containing registers and hardware configuration. |
| [`NexBank`](../../api/MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex/NexBank/index.md)     | A 16 KB RAM bank.                                                |
| [`NexScreen`](../../api/MrKWatkins.OakIO.ZXSpectrumNext.Snapshot.Nex/NexScreen/index.md) | An attached screen buffer.                                       |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("snapshot.nex");
NexFile nex = NexFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.nex");
nex.Write(output);
```

## Appended data

The [NEX specification](https://wiki.specnext.dev/NEX_file_format) permits application-defined binary data after the final RAM bank. The loader leaves this data in the file instead of loading it into memory. Its size does not increase the declared bank count or RAM requirement.

`NexFormat` preserves all of these bytes when reading and writing, including with asynchronous or non-seekable streams. `NexFile.AppendedData` exposes them as a read-only list; a file without appended data returns an empty list. OakIO treats the bytes as opaque data and does not impose an archive format or interpret `NumExtraFiles` as a resource count.

```c#
var nex = NexFile.CreateCode([(2, [0xF3, 0x76])], 0x8000, 0xFF00);

// Copies the supplied bytes, replacing any existing suffix.
nex.SetAppendedData(File.ReadAllBytes("assets.bin"));

// Keep the NEX open and pass its handle to the program in BC.
nex.Header.FileHandleAddress = 1;

// File-relative offset, calculated from the header, palette, screens, copper and banks.
long offset = nex.AppendedDataOffset;
int length = nex.AppendedData.Count;

using var output = File.Create("game.nex");
nex.Write(output);
```

`AppendedDataOffset` is relative to the beginning of the NEX header, regardless of the stream's starting position. It also gives the append position when the suffix is empty. Appended data can exceed 64 KB or the target's RAM capacity; the application can read it in smaller pieces. Use `SetAppendedData([])` to remove it. Editing data preserves existing checksum fields without recalculating them.

`FileHandleAddress` controls loader behaviour independently of the suffix:

| Value | Loader behaviour |
|-------|------------------|
| `0` | Closes the file before starting the program. |
| `1` to `0x3FFF` | Keeps it open and returns its handle in BC; `1` is recommended. |
| `0x4000` to `0xFFFF` | Keeps it open and writes its handle at that CPU address, after mapping the entry bank. |

The application owns the retained handle and must close it when finished.

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
{
    Console.WriteLine($"Bank {bank.BankNumber}: {bank.Data.Count} bytes");
}

// Screens
foreach (NexScreen screen in nex.Screens)
{
    Console.WriteLine($"Screen: {screen.Type}");
}
```
