# SPR

SPR files are headerless collections of sprite patterns. OakIO supports 16×16 row-major sprite patterns at an explicitly selected bit depth. Palettes are separate; no header, palette or display attributes are stored in these files.

The [Next sprite upload specification](https://wiki.specnext.dev/Sprite_Pattern_Upload) documents the pattern dimensions, reading order and four-bit packing. [Gfx2Next](https://github.com/benbaker76/Gfx2Next) exports sprite collections with the `.spr` extension.

## API

These types are in the `MrKWatkins.OakIO.ZXSpectrumNext` package.

| Class | Description |
| --- | --- |
| [`SprFormat`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr/SprFormat/index.md) | Reads and writes files at an explicitly selected bit depth. |
| [`SprFile`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr/SprFile/index.md) | Ordered byte-backed sprite patterns, with a derived `TilesData` view. |
| [`SprPattern`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr/SprPattern/index.md) | One stored sprite pattern with raw bytes and unpacked pixel indices. |
| [`ImageToSprConverter`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr/ImageToSprConverter/index.md) | Splits an indexed image into 16×16 sprite patterns. |
| [`TilesToSprConverter`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr/TilesToSprConverter/index.md) | Encodes 16×16 tiles at the selected bit depth. |

## Explicit Interpretation

| Bits per pixel | Bytes per pattern | Format |
| --- | --- | --- |
| 4 | 128 | `SprFormat.FourBit` |
| 8 | 256 | `SprFormat.EightBit` |

There is no default `Instance`. Use a named format or `SprFormat.ForBitsPerPixel(bitsPerPixel)`. Unsupported bit depths throw `ArgumentOutOfRangeException`.

Length cannot determine the encoding: the same bytes can represent twice as many four-bit sprite patterns as eight-bit sprite patterns. The selected bit depth is interpretation metadata, not an on-disk field. Files must contain complete 16×16 sprite patterns; partial data throws `InvalidDataException`. Empty collections are allowed. Asset collections are not limited to the number of patterns that fit in hardware at once.

This API handles standard 16×16 sprite patterns, not font data or other layouts that tools may also name `.spr`. Headerless bytes cannot identify such variants automatically.

## Reading and Writing

```c#
using var input = File.OpenRead("sprites.spr");
SprFile file = SprFormat.FourBit.Read(input);

using var output = File.Create("output.spr");
file.Write(output);
```

Synchronous and asynchronous reads and writes preserve all stored bytes and tile order. Compression uses the normal file APIs; see [Reading, Writing and Converting](../../reading-writing-converting.md).

Unqualified `ZXSpectrumNextResourceFileFormats.AllFormats` excludes SPR and NXT because their bit depths must be supplied. For discovery with explicit modes:

```c#
var formats = ZXSpectrumNextResourceFileFormats.WithTiles(
    spriteBitsPerPixel: 4, tileBitsPerPixel: 4);
using var input = File.OpenRead("sprites.spr");
IOFile file = IOFileFormat.Load("sprites.spr", input, formats);
```

## Components and Packing

`Patterns` exposes the ordered `SprPattern` components. Each exposes its original `Data`, its selected `BitsPerPixel` and derived `Pixels` in row-major order. `Tiles` derives a shared `TilesData` view in tile-major order; it is not an independently stored copy.

Within each pattern, rows run from top to bottom and pixels from left to right. Four-bit data stores two indices per byte: the high nibble is the left pixel and the low nibble the right. Eight-bit data stores one index per byte. There is no row padding.

## Creating Files

```c#
var pixels = Enumerable.Repeat((byte)15, 256).ToArray();
var tiles = new TilesData(16, 16, pixels, bitsPerPixel: 4);
var file = new SprFile(tiles);
```

Creation uses the explicit bit depth in `TilesData`, and preserves indices, order and duplicates. The tile dimensions must be 16×16. There is no cropping, deduplication, palette generation or colour remapping.

## Conversions

```c#
// bmp must contain an indexed image with dimensions divisible by 16.
var converter = new ImageToSprConverter(BmpFormat.Instance, SprFormat.FourBit);
SprFile file = converter.Convert(bmp);

// Re-encode without changing the pixel indices.
var wider = new TilesToSprConverter(file.Format, SprFormat.EightBit).Convert(file);
```

Image conversion orders sprite patterns left to right, then top to bottom across the source image. Each pattern retains its own row-major pixel order. Non-indexed images throw `NotSupportedException`; misaligned image dimensions and mismatched source tile dimensions throw `ArgumentException`.

Widening preserves indices. Narrowing succeeds only when every index fits the selected bit depth; otherwise it throws `ArgumentException`. No indices are truncated or remapped. Palettes remain external and must be retained separately by the caller.

These converters are selected explicitly rather than globally registered with `IOFileConversion`: several encodings share the same file type, so a target file type alone cannot identify the desired bit depth. CLI and online converter integration remains deferred.
