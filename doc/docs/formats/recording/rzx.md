# RZX

RZX is an input recording format for the ZX Spectrum. Rather than storing sound or timing data like the tape formats, it records the fetch counts and I/O port reads needed to replay a session deterministically against an emulated machine, optionally alongside an embedded snapshot of the state to start from.

An RZX file is a sequence of typed blocks: creator information identifying the tool that made the recording, an optional snapshot, and one or more input recording blocks containing the recorded frames.

Details about the RZX format can be found at [https://worldofspectrum.net/RZXformat.html](https://worldofspectrum.net/RZXformat.html).

## API

| Class                                                                                                           | Description                                                         |
|-----------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------|
| [`RzxFormat`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxFormat/index.md)                           | Singleton format for reading and writing RZX files.                 |
| [`RzxFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxFile/index.md)                               | Represents an RZX file as a header and a list of blocks.            |
| [`RzxHeader`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxHeader/index.md)                           | The file header, including the RZX version.                         |
| [`RzxBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxBlock/index.md)                             | Base class for all RZX blocks.                                      |
| [`RzxCreatorBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxCreatorBlock/index.md)               | Identifies the tool that created the recording.                     |
| [`RzxSnapshotBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxSnapshotBlock/index.md)             | An embedded snapshot to start replay from.                          |
| [`RzxInputRecordingBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxInputRecordingBlock/index.md) | A recording of input frames.                                        |
| [`RzxInputFrame`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxInputFrame/index.md)                   | A single frame's fetch count and I/O port reads within a recording. |

## Reading and Writing

```c#
// Read
using var stream = File.OpenRead("recording.rzx");
RzxFile rzx = RzxFormat.Instance.Read(stream);

// Write
using var output = File.Create("output.rzx");
rzx.Write(output);
```

Security blocks (used to sign competition entries) are not supported. Reading a file containing one throws a `NotSupportedException`.

## Accessing Blocks

An [`RzxFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxFile/index.md) exposes its raw [`Blocks`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxFile/Blocks.md) list, plus convenience properties for the block types every valid recording has:

```c#
Console.WriteLine($"Created by: {rzx.Creator.Creator} {rzx.Creator.MajorVersion}.{rzx.Creator.MinorVersion}");

if (rzx.Snapshot is { } snapshot)
{
    Console.WriteLine($"Starting snapshot: .{snapshot.Extension}, {snapshot.SnapshotData.Count} bytes");
}

foreach (var recording in rzx.InputRecordings)
{
    Console.WriteLine($"Recording: {recording.Frames.Count} frames from T-state {recording.StartTStates}");
}
```

[`Creator`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxFile/Creator.md) throws if the file has no creator block; [`Snapshot`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxFile/Snapshot.md) is `null` if the file has none.

Each [`RzxInputFrame`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxInputFrame/index.md) carries the number of instruction fetches performed and the bytes returned by I/O port reads during that frame, or indicates that it repeats the previous frame's port reads unchanged:

```c#
foreach (var frame in recording.Frames)
{
    if (frame.RepeatsPreviousInputReads)
    {
        Console.WriteLine($"{frame.FetchCount} fetches, repeats previous reads");
    }
    else
    {
        Console.WriteLine($"{frame.FetchCount} fetches, {frame.InputReads.Count} port reads");
    }
}
```

## Creating RZX Files

Build up the blocks and pass them to the [`RzxFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxFile/index.md) constructor. A creator block and at least one input recording block are required:

```c#
var creator = new RzxCreatorBlock("OakIO", majorVersion: 1, minorVersion: 0);

var frames = new[]
{
    new RzxInputFrame(fetchCount: 1000, inputReads: [0xFF]),
    new RzxInputFrame(fetchCount: 1000, repeatsPreviousInputReads: true),
};
var recording = new RzxInputRecordingBlock(frames);

RzxFile rzx = new([creator, recording]);
```

An optional [`RzxSnapshotBlock`](../../API/MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx/RzxSnapshotBlock/index.md) embeds a snapshot to start replay from, identified by its filename extension:

```c#
byte[] snapshotBytes = z80.ToByteArray();
var snapshot = new RzxSnapshotBlock("z80", snapshotBytes);

RzxFile rzx = new([creator, snapshot, recording]);
```

Compressed and external-data snapshot blocks, and protected input recording blocks, can be read but not created — only uncompressed, embedded, unprotected blocks can be written.

## Conversions

RZX recordings do not participate in conversion — no converters are registered to or from the format. See [Reading, Writing and Converting](../../reading-writing-converting.md).
