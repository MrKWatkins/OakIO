# NXP

NXP is a headerless ZX Spectrum Next palette format. It contains either 16 RGB333 colours (32 bytes) or 256 RGB333 colours (512 bytes). Each entry stores the upper eight colour bits in an RGB332 byte followed by a zero-extended byte containing the lowest blue bit.

The format is documented by the [Spectrum Next BMP tools](https://github.com/stefanbylund/zxnext_bmp_tools#nextraw), whose `nextraw` tool writes separate palettes with the `.nxp` extension.

## API

These types are in the `MrKWatkins.OakIO.ZXSpectrumNext` package.

| Class | Description |
| --- | --- |
| [`NxpFormat`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp/NxpFormat/index.md) | Singleton format for reading and writing NXP palettes. |
| [`NxpFile`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp/NxpFile/index.md) | Headerless palette with ordered byte-backed entries. |
| [`NxpColourEntry`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp/NxpColourEntry/index.md) | Two-byte entry exposing RGB333 components and an RGB8 view. |
| [`PaletteToNxpConverter`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp/PaletteToNxpConverter/index.md) | Converts an opaque palette with 16 or 256 entries to RGB333. |

## Reading and Writing

```c#
using var input = File.OpenRead("palette.nxp");
NxpFile file = NxpFormat.Instance.Read(input);

using var output = File.Create("output.nxp");
file.Write(output);
```

Reads and writes preserve all entry bytes, ordering and duplicate colours. There is no header or other metadata. Unsupported lengths and second bytes containing bits other than bit zero throw `InvalidDataException`. An eight-bit RGB332 palette containing 256 bytes is not an NXP file and is rejected.

The format also supports asynchronous I/O and compression. For resource discovery, use [`ZXSpectrumNextResourceFileFormats.AllFormats`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources/ZXSpectrumNextResourceFileFormats/index.md), which includes general resources and NXP. Resource formats remain separate from the snapshot and tape discovery lists; CLI and online converter integration is deferred.

## Components

`Entries` exposes the ordered `NxpColourEntry` components. Each entry exposes its original two bytes through `Data`, and its three-bit `Red`, `Green` and `Blue` values (0–7). `Colour` expands these to opaque RGB8 values.

`Palette` derives a shared `PaletteData` view from the entries rather than retaining an independently stored copy. RGB333 levels expand to the nearest RGB8 values:

| RGB333 component | RGB8 component |
| --- | --- |
| 0 | 0 |
| 1 | 36 |
| 2 | 73 |
| 3 | 109 |
| 4 | 146 |
| 5 | 182 |
| 6 | 219 |
| 7 | 255 |

## Creating Palettes

```c#
var palette = new PaletteData(Enumerable.Repeat(new Colour(255, 128, 36), 16));
var file = new NxpFile(palette);
byte[] bytes = file.ToByteArray();
```

Creation requires exactly 16 or 256 opaque colours. RGB8 components are rounded to the nearest RGB333 level, so values may change: for example, green 128 becomes level four and expands back to 146. Creating an entry from an expanded RGB333 colour recovers its original bytes. Index order and duplicate entries are retained; colours are not deduplicated, and palettes are not padded or truncated.

Unsupported colour counts and non-opaque colours throw `ArgumentException`. NXP has no alpha channel; transparency is configured separately by the graphics hardware, not encoded in this palette.

## Conversions

GPL, JASC and Paint.NET palettes can be converted to NXP when they contain exactly 16 or 256 opaque entries. NXP can be converted back to all three formats:

```c#
var palette = new PaletteData(Enumerable.Repeat(new Colour(255, 128, 36), 16));
var source = new GplFile(palette);
NxpFile nxp = IOFileConversion.Convert<NxpFile>(source);
PaintNetFile text = IOFileConversion.Convert<PaintNetFile>(nxp);
```

The Next assembly registers the incoming conversions when it is loaded; no prior NXP file construction is needed. Outgoing conversions use the usual file-format registration mechanism. The core package has no dependency on Next, and converter base constructors remain `private protected`.

Conversions preserve index order and duplicate entries. Incoming conversions round RGB8 components as described above; outgoing conversions expand RGB333 to RGB8 and create canonical text files. Comments and names are not transferred, and GPL output uses its constructor defaults. See [Reading, Writing and Converting](../../reading-writing-converting.md) for the conversion API.
