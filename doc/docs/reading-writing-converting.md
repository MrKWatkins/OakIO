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

The returned object is a strongly-typed subclass of [`IOFile`](API/MrKWatkins.OakIO/IOFile/index.md) specific to the format.

## Writing Files

Pass the file object and a [`Stream`](https://learn.microsoft.com/en-us/dotnet/api/system.io.stream) to [`Write`](API/MrKWatkins.OakIO/IOFileFormat/Write.md):

```c#
using var stream = File.Create("output.tap");
TapFormat.Instance.Write(tap, stream);
```

There are also overloads that write directly to a file path, or return a `byte[]`:

```c#
// Write to a file path:
TapFormat.Instance.Write(tap, "/path/to/output.tap");

// Write to a byte array:
byte[] bytes = TapFormat.Instance.Write(tap);
```

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
    TapFormat.Instance.Write(result, "output.tap");
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
