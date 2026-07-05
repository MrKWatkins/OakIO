# Reading, Writing and Converting

## Reading Files

Each format exposes a singleton [`IOFileFormat`](API/MrKWatkins.OakIO/IOFileFormat/index.md) instance via its `Instance` property. Call [`Read`](API/MrKWatkins.OakIO/IOFileFormat/Read.md) with a [`Stream`](https://learn.microsoft.com/en-us/dotnet/api/system.io.stream) or a `byte[]`:

```c#
using var stream = File.OpenRead("tape.tap");
TapFile tap = TapFormat.Instance.Read(stream);

// Or from a byte array:
byte[] bytes = File.ReadAllBytes("tape.tap");
TapFile tap = TapFormat.Instance.Read(bytes);
```

The returned object is a strongly typed subclass of [`IOFile`](API/MrKWatkins.OakIO/IOFile/index.md) specific to the format.

If you don't know the format ahead of time, use [`IOFileFormat.Load`](API/MrKWatkins.OakIO/IOFileFormat/Load.md) with the set of formats you support — it picks the right one based on the file's extension:

```c#
IOFile file = IOFileFormat.Load("tape.tap", TapFormat.Instance, TzxFormat.Instance, PzxFormat.Instance);
```

[`Load`](API/MrKWatkins.OakIO/IOFileFormat/Load.md) also transparently decompresses `.zip`, `.gz`, `.br`, and `.zst` files — see [Compression](#compression) below.

## Writing Files

An [`IOFile`](API/MrKWatkins.OakIO/IOFile/index.md) writes itself — pass a [`Stream`](https://learn.microsoft.com/en-us/dotnet/api/system.io.stream) to [`Write`](API/MrKWatkins.OakIO/IOFile/Write.md):

```c#
using var stream = File.Create("output.tap");
tap.Write(stream);
```

Use [`ToByteArray`](API/MrKWatkins.OakIO/IOFile/ToByteArray.md) to get a `byte[]` instead, or [`Save`](API/MrKWatkins.OakIO/IOFile/Save.md) to write straight to a directory — it works out the filename, appending the format's extension automatically:

```c#
byte[] bytes = tap.ToByteArray();

// Writes to "/path/to/output.tap":
string path = tap.Save("/path/to", "output");
```

## Reading and Writing Asynchronously

Every reading and writing method has an asynchronous counterpart — [`ReadAsync`](API/MrKWatkins.OakIO/IOFileFormat/ReadAsync.md), [`LoadAsync`](API/MrKWatkins.OakIO/IOFileFormat/LoadAsync.md), [`WriteAsync`](API/MrKWatkins.OakIO/IOFile/WriteAsync.md), and [`SaveAsync`](API/MrKWatkins.OakIO/IOFile/SaveAsync.md) — each taking an optional `CancellationToken`:

```c#
using var stream = File.OpenRead("tape.tap");
TapFile tap = await TapFormat.Instance.ReadAsync(stream);

using var output = File.Create("output.tap");
await tap.WriteAsync(output);

string path = await tap.SaveAsync("/path/to", "output");
```

## Compression

Files can be written compressed by passing a [`CompressionFormat`](API/MrKWatkins.OakIO.Compression/CompressionFormat/index.md) to [`Write`](API/MrKWatkins.OakIO/IOFile/Write.md), [`WriteAsync`](API/MrKWatkins.OakIO/IOFile/WriteAsync.md), [`Save`](API/MrKWatkins.OakIO/IOFile/Save.md), or [`SaveAsync`](API/MrKWatkins.OakIO/IOFile/SaveAsync.md). Supported formats are `Zip`, `GZip`, `Brotli`, and `Zstandard`:

```c#
using var stream = File.Create("output.wav.gz");
wav.Write(stream, "output.wav", CompressionFormat.GZip);

// Or when saving to a directory, which works out the compressed filename automatically:
string path = wav.Save("/path/to", "output", CompressionFormat.GZip); // "/path/to/output.wav.gz"
```

`Zip` compression stores the file as a single entry inside a `.zip` archive, using the `filename` argument as the name of that entry. The other formats append their own extension (`.gz`, `.br`, `.zst`) to the filename instead.

[`Load`](API/MrKWatkins.OakIO/IOFileFormat/Load.md) and [`LoadAsync`](API/MrKWatkins.OakIO/IOFileFormat/LoadAsync.md) transparently decompress a file based on its extension, so no special handling is needed to read a compressed file back.

## Converting Between Formats

[`IOFileConversion`](API/MrKWatkins.OakIO/IOFileConversion/index.md) provides static methods to convert a file from one format to another. Conversions are registered by each format and can be discovered at runtime.

### Converting to a Known Type

Use the generic [`Convert<TTarget>`](API/MrKWatkins.OakIO/IOFileConversion/Convert.md) overload when you know the target type at compile time:

```c#
TapFile tap = TapFormat.Instance.Read(stream);

// Convert TAP → TZX:
TzxFile tzx = IOFileConversion.Convert<TzxFile>(tap);

// Convert TAP → PZX:
PzxFile pzx = IOFileConversion.Convert<PzxFile>(tap);

// Convert TZX → TAP:
TapFile tapFromTzx = IOFileConversion.Convert<TapFile>(tzx);
```

### Converting with Error Handling

Use [`TryConvert`](API/MrKWatkins.OakIO/IOFileConversion/TryConvert.md) when the conversion might fail. This is useful for conversions like TZX → TAP or PZX → TAP where not all block types can be represented:

```c#
if (IOFileConversion.TryConvert(tzx, TapFormat.Instance, out var result, out var error))
{
    result.Write(stream);
}
else
{
    Console.WriteLine($"Conversion failed: {error}");
}
```

### Converting to WAV

Use [`ConvertToWav`](API/MrKWatkins.OakIO/IOFileConversion/ConvertToWav.md) to produce a WAV audio file from any tape format. An optional sample rate can be specified (default is 44100 Hz):

```c#
WavFile wav = IOFileConversion.ConvertToWav(tap);

// With a custom sample rate:
WavFile wav = IOFileConversion.ConvertToWav(tap, sampleRateHz: 48000);
```

### Discovering Supported Conversions

Use [`GetSupportedConversionFormats`](API/MrKWatkins.OakIO/IOFileConversion/GetSupportedConversionFormats.md) to discover which target formats are available for a given source:

```c#
IReadOnlyList<IOFileFormat> targets = IOFileConversion.GetSupportedConversionFormats(TapFormat.Instance);
foreach (var format in targets)
{
    Console.WriteLine(format.Name);
}
```

Note that [RZX](formats/recording/rzx.md) recordings do not participate in conversion — no converters are registered to or from the format.
