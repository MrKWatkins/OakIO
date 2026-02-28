# TAP Tape

TAP is a simple tape format for the ZX Spectrum. A TAP file is a flat sequence of blocks, each consisting of a flag byte, raw data bytes, and a checksum byte. There is no metadata about the recording itself — it is purely the data that would have been read from or written to tape.

Details about the SNA format can be found at [https://sinclair.wiki.zxnet.co.uk/wiki/TAP_format](https://sinclair.wiki.zxnet.co.uk/wiki/TAP_format).

## API

| Class                                                                             | Description                                         |
|-----------------------------------------------------------------------------------|-----------------------------------------------------|
| [`TapFormat`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapFormat/index.md)     | Singleton format for reading and writing TAP files. |
| [`TapFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapFile/index.md)         | Represents a TAP file as a list of blocks.          |
| [`TapBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapBlock/index.md)       | Base class for all TAP blocks.                      |
| [`HeaderBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/HeaderBlock/index.md) | A header block describing the file that follows.    |
| [`DataBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/DataBlock/index.md)     | A data block containing the file contents.          |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("tape.tap");
TapFile tap = TapFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.tap");
TapFormat.Instance.Write(tap, output);
```

## Blocks

A [`TapFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapFile/index.md) contains a list of [`TapBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapBlock/index.md) objects. Each block is either a [`HeaderBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/HeaderBlock/index.md) or a [`DataBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/DataBlock/index.md):

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

[`HeaderBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/HeaderBlock/index.md) carries a [`TapHeaderType`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapHeaderType/index.md) (Program, NumberArray, CharacterArray, or Code), a filename, and format-specific fields such as `DataBlockLength` and `Location`.

## Creating TAP Files

Factory methods on [`HeaderBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/HeaderBlock/index.md) and [`DataBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/DataBlock/index.md) create individual blocks, and factory methods on [`TapFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tap/TapFile/index.md) create a file with correctly-paired header/data block sequences:

```c#
// A code block at location 0x8000
TapFile code = TapFile.CreateCode("game", 0x8000, myBytes);

// A loader that auto-runs and loads code blocks
TapFile loader = TapFile.CreateLoader("loader", (0x8000, myBytes));
```

## Conversions

TAP files can be converted to TZX, PZX, and WAV. See [Reading, Writing and Converting](../reading-writing-converting.md).
