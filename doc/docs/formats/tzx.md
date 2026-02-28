# TZX Format

TZX is the standard format for preserving ZX Spectrum tape recordings. Unlike [TAP](tap.md), TZX encodes the timing of pulses on the tape, making it suitable for non-standard loaders, copy-protected software, and archive metadata. TZX files contain a sequence of typed blocks, each with its own structure.

Details about the TZX format can be found at [https://worldofspectrum.net/TZXformat.html](https://worldofspectrum.net/TZXformat.html).

## API

| Class                                                                         | Description                                               |
|-------------------------------------------------------------------------------|-----------------------------------------------------------|
| [`TzxFormat`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/TzxFormat/index.md) | Singleton format for reading and writing TZX files.       |
| [`TzxFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/TzxFile/index.md)     | Represents a TZX file with a header and a list of blocks. |
| [`TzxBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/TzxBlock/index.md)   | Base class for all TZX blocks.                            |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("tape.tzx");
TzxFile tzx = TzxFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.tzx");
TzxFormat.Instance.Write(tzx, output);
```

## Blocks

A [`TzxFile`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/TzxFile/index.md) contains a list of [`TzxBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/TzxBlock/index.md) objects. Each block is a strongly-typed subclass:

| Block Type                                                                                                                                                                        | Description                                        |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------|
| [`StandardSpeedDataBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/StandardSpeedDataBlock/index.md)                                                                           | Standard ROM timing (most common).                 |
| [`TurboSpeedDataBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/TurboSpeedDataBlock/index.md)                                                                                 | Custom timing for turbo loaders.                   |
| [`PureToneBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/PureToneBlock/index.md)                                                                                             | A repeating tone pulse.                            |
| [`PulseSequenceBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/PulseSequenceBlock/index.md)                                                                                   | An arbitrary sequence of pulse lengths.            |
| [`PureDataBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/PureDataBlock/index.md)                                                                                             | Data without leader/sync pulses.                   |
| [`PauseBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/PauseBlock/index.md)                                                                                                   | A pause or stop signal.                            |
| [`GroupStartBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/GroupStartBlock/index.md) / [`GroupEndBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/GroupEndBlock/index.md) | Named group of blocks.                             |
| [`LoopStartBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/LoopStartBlock/index.md) / [`LoopEndBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/LoopEndBlock/index.md)     | Repeated block sequence.                           |
| [`TextDescriptionBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/TextDescriptionBlock/index.md)                                                                               | Inline text comment.                               |
| [`StopTheTapeIf48KBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/StopTheTapeIf48KBlock/index.md)                                                                             | Conditional stop for 48K machines.                 |
| [`ArchiveInfoBlock`](../API/MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx/ArchiveInfoBlock/index.md)                                                                                       | Archive metadata (title, author, publisher, etc.). |

```c#
foreach (TzxBlock block in tzx.Blocks)
{
    switch (block)
    {
        case StandardSpeedDataBlock data:
            Console.WriteLine($"Data: {data.Header.BlockLength} bytes, pause {data.Header.PauseAfterBlockMs} ms");
            break;
        case ArchiveInfoBlock info:
            foreach (var entry in info.Entries)
                Console.WriteLine($"{entry.Type}: {entry.Text}");
            break;
    }
}
```

## Conversions

TZX files can be converted to PZX and WAV. See [Reading, Writing and Converting](../reading-writing-converting.md).
