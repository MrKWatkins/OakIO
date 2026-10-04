# NXT

NXT files are headerless collections of tiles. OakIO supports 8×8 row-major tiles at an explicitly selected bit depth. Palettes are separate; no header, palette or display attributes are stored in these files.

[Gfx2Next](https://github.com/benbaker76/Gfx2Next) exports `.nxt` tiles and supports one-, four- and eight-bit indices. Its [source code](https://github.com/benbaker76/Gfx2Next/blob/master/src/gfx2next.c) documents MSB-first one-bit packing and high-nibble-first four-bit packing.

## API

These types are in the `MrKWatkins.OakIO.ZXSpectrumNext` package.

| Class | Description |
| --- | --- |
| [`NxtFormat`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt/NxtFormat/index.md) | Reads and writes files at an explicitly selected bit depth. |
| [`NxtFile`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt/NxtFile/index.md) | Ordered byte-backed tiles, with a derived `TilesData` view. |
| [`NxtTile`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt/NxtTile/index.md) | One stored tile with raw bytes and unpacked pixel indices. |
| [`ImageToNxtConverter`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt/ImageToNxtConverter/index.md) | Splits an indexed image into 8×8 tiles. |
| [`TilesToNxtConverter`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt/TilesToNxtConverter/index.md) | Encodes 8×8 tiles at the selected bit depth. |

## Explicit Interpretation

| Bits per pixel | Bytes per tile | Format |
| --- | --- | --- |
| 1 | 8 | `NxtFormat.OneBit` |
| 4 | 32 | `NxtFormat.FourBit` |
| 8 | 64 | `NxtFormat.EightBit` |

No default `Instance` is provided. Use a named format or `NxtFormat.ForBitsPerPixel(bitsPerPixel)`. Unsupported bit depths throw `ArgumentOutOfRangeException`.

Length cannot determine the encoding: the same bytes can represent twice as many four-bit tiles as eight-bit tiles. The selected bit depth is interpretation metadata, not an on-disk field. Files must contain complete 8×8 tiles; partial data throws `InvalidDataException`. Empty collections are allowed. Asset collections are not limited to the number of patterns that fit in hardware at once.

This first version interprets NXT as 8×8 row-major chunky tiles. Gfx2Next can also export other dimensions, column-major or planar layouts. Those layouts are not supported by this API, and headerless bytes cannot identify them automatically. Eight-bit NXT is an asset/export encoding; the [Next hardware tilemap](https://wiki.specnext.dev/Tilemap) uses one- or four-bit tiles.

## Reading and Writing

```c#
using var input = File.OpenRead("tiles.nxt");
NxtFile file = NxtFormat.FourBit.Read(input);

using var output = File.Create("output.nxt");
file.Write(output);
```

Synchronous and asynchronous reads and writes preserve all stored bytes and tile order. Compression uses the normal file APIs; see [Reading, Writing and Converting](../../reading-writing-converting.md).

Unqualified `ZXSpectrumNextResourceFileFormats.AllFormats` excludes SPR and NXT because their bit depths must be supplied. For discovery with explicit modes:

```c#
var formats = ZXSpectrumNextResourceFileFormats.WithTiles(
    spriteBitsPerPixel: 4, tileBitsPerPixel: 4);
using var input = File.OpenRead("tiles.nxt");
IOFile file = IOFileFormat.Load("tiles.nxt", input, formats);
```

## Components and Packing

`Entries` exposes the ordered `NxtTile` components. Each exposes its original `Data`, its selected `BitsPerPixel` and derived `Pixels` in row-major order. `Tiles` derives a shared `TilesData` view in tile-major order; it is not an independently stored copy.

Within each tile, rows run from top to bottom and pixels from left to right. Four-bit data stores two indices per byte: the high nibble is the left pixel and the low nibble the right. Eight-bit data stores one index per byte. One-bit data stores 8 pixels per byte, with the left pixel in bit seven. Rows have no padding.

## Creating Files

```c#
var pixels = Enumerable.Repeat((byte)15, 64).ToArray();
var tiles = new TilesData(8, 8, pixels, bitsPerPixel: 4);
var file = new NxtFile(tiles);
```

Creation uses the explicit bit depth in `TilesData`, and preserves indices, order and duplicates. The tile dimensions must be 8×8. Creation performs no cropping, deduplication, palette generation or colour remapping.

## Conversions

```c#
// bmp must contain an indexed image with dimensions divisible by 8.
var converter = new ImageToNxtConverter(BmpFormat.Instance, NxtFormat.FourBit);
NxtFile file = converter.Convert(bmp);

// Re-encode without changing the pixel indices.
var wider = new TilesToNxtConverter(file.Format, NxtFormat.EightBit).Convert(file);
```

Image conversion orders tiles left to right, then top to bottom across the source image. Each tile retains its own row-major pixel order. Non-indexed images throw `NotSupportedException`; misaligned image dimensions and mismatched source tile dimensions throw `ArgumentException`.

Widening preserves indices. Narrowing succeeds only when every index fits the selected bit depth; otherwise it throws `ArgumentException`. No indices are truncated or remapped. Palettes remain external and must be retained separately by the caller.

These converters are selected explicitly rather than globally registered with `IOFileConversion`: several encodings share the same file type, so a target file type alone cannot identify the desired bit depth. CLI and online converter integration remains deferred.
