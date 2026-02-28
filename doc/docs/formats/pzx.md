# PZX Tape

PZX is an alternative tape format for the ZX Spectrum with pulse-level encoding. It uses a tagged block structure with four-byte ASCII block identifiers. PZX is designed to be simpler and more compact than [TZX](tzx.md) while still preserving the full timing information needed for non-standard loaders.

Details about the PZX format can be found at [https://github.com/raxoft/pzxtools](https://github.com/raxoft/pzxtools).

## API

| Class                                                                         | Description                                         |
|-------------------------------------------------------------------------------|-----------------------------------------------------|
| [`PzxFormat`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PzxFormat/index.md) | Singleton format for reading and writing PZX files. |
| [`PzxFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PzxFile/index.md)     | Represents a PZX file as a list of blocks.          |
| [`PzxBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PzxBlock/index.md)   | Base class for all PZX blocks.                      |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("tape.pzx");
PzxFile pzx = PzxFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.pzx");
PzxFormat.Instance.Write(pzx, output);
```

## Blocks

A [`PzxFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PzxFile/index.md) contains a list of [`PzxBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PzxBlock/index.md) objects. The first block is always a [`PzxHeaderBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PzxHeaderBlock/index.md) (`PZXT` tag) carrying version information and optional metadata entries.

| Block Type                                                                                      | Tag    | Description                                        |
|-------------------------------------------------------------------------------------------------|--------|----------------------------------------------------|
| [`PzxHeaderBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PzxHeaderBlock/index.md)         | `PZXT` | File header with version and metadata.             |
| [`PulseSequenceBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PulseSequenceBlock/index.md) | `PULS` | Sequence of pulse lengths in T-states.             |
| [`DataBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/DataBlock/index.md)                   | `DATA` | Data bits with associated zero/one pulse patterns. |
| [`PauseBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/PauseBlock/index.md)                 | `PAUS` | A pause of a given duration in T-states.           |
| [`BrowsePointBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/BrowsePointBlock/index.md)     | `BRWS` | A named browse point for tape navigation.          |
| [`StopBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx/StopBlock/index.md)                   | `STOP` | Stop the tape, optionally only on 48K machines.    |

```c#
if (pzx.Blocks[0] is PzxHeaderBlock header)
{
    Console.WriteLine($"PZX version {header.Header.MajorVersionNumber}.{header.Header.MinorVersionNumber}");
    foreach (var info in header.Info)
        Console.WriteLine($"{info.Type}: {info.Text}");
}

foreach (PzxBlock block in pzx.Blocks.Skip(1))
{
    switch (block)
    {
        case DataBlock data:
            Console.WriteLine($"Data: {data.Header.SizeInBytes} bytes");
            break;
        case PauseBlock pause:
            Console.WriteLine($"Pause: {pause.Header.Duration} T-states");
            break;
    }
}
```

## Conversions

PZX files can be converted to TAP, TZX, and WAV.

When converting to TAP, only `DataBlock` blocks are converted. All other block types (header, pulse sequences, pauses, browse points, and stop blocks) are skipped.

See [Reading, Writing and Converting](../reading-writing-converting.md).
