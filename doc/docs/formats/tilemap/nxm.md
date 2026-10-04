# NXM

`MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm` reads and writes headerless tile or block maps.
NXM contains only map entries: dimensions, entry interpretation and traversal order must be supplied externally.
It contains no tile graphics, palette, layer metadata or invented header.

References: [Gfx2Next options](https://github.com/benbaker76/Gfx2Next) and its
[pinned map writer](https://github.com/benbaker76/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c)
(`write_map`, `get_tile` and `convert_tiles`), plus the [Next tilemap specification](https://wiki.specnext.dev/Tilemap).
The writer and hardware references are also linked in code comments.

## Explicit layouts

[`NxmFormat`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm/NxmFormat/index.md) requires width
and height in **map entries**, an encoding and an optional `ResourceDataOrder` (default: row-major).
The default order traverses left to right, then top to bottom; column-major traverses top to bottom, then
left to right. Both start at the top-left. Two-byte entries are little-endian.

| `NxmEncoding` | Bytes per entry | Index range | Attributes |
| --- | --- | --- | --- |
| `Index8` | 1 | 0–255 | None |
| `Index16` | 2 | 0–65535 | None |
| `Tiles256` | 2 | 0–255 | Palette offset 0–15, X/Y mirrors, rotation, priority |
| `Tiles512` | 2 | 0–511 | Palette offset 0–15, X/Y mirrors, rotation |
| `Text256` | 2 | 0–255 | Palette offset 0–127, priority |
| `Text512` | 2 | 0–511 | Palette offset 0–127 |

Graphics entries are tile byte followed by `PPPPXYRI`; text entries are tile byte followed by `PPPPPPPI`.
In 512-tile modes, `I` is tile index bit eight. In 256-tile modes, it selects ULA over tilemap and is exposed
as `Priority`. These hardware interpretations require explicit selection; nothing in the file identifies
them. Text mode does not have mirror/rotation flags. An eight-bit map has no per-entry attributes;
hardware global attributes are outside the file.

Gfx2Next's ordinary `-map-16bit` tile output uses the 512-tile graphics encoding, not a plain sixteen-bit
index. Maps referencing reusable NXB blocks can instead use plain indices: choose `Index8` or `Index16`.
The interpretation is not guessed from length or from the highest index. File length must be exactly
`width * height * bytesPerEntry`; short, empty and trailing-data files throw `InvalidDataException`.
There is no restriction to the hardware's visible map dimensions: larger scrolling assets are supported.

## Reading and writing

```csharp
var format = new NxmFormat(40, 32, NxmEncoding.Tiles512);
NxmFile file = format.Read(bytes);
byte[] original = file.ToByteArray();
await file.WriteAsync(outputStream);

ResourceFormat[] formats = [.. ZXSpectrumNextResourceFileFormats.AllFormats, format];
var loaded = await IOFileFormat.LoadAsync("map.nxm", inputStream, formats);
```

Unqualified format lists exclude NXM because they cannot supply its layout. Add one chosen format to
discovery explicitly. Synchronous, asynchronous and existing OakIO compression APIs are supported.
Gfx2Next's optional ZX0 wrapper and non-Next Sega Master System attribute encoding are not handled by
this native format. A raw file of the right length is assumed to use the chosen export settings.

[`NxmFile`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm/NxmFile/index.md) retains its layout
in `Format` and its native bytes in the byte-backed
[`MapData`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm/NxmMapData/index.md) component.
Input bytes are copied; reading/writing preserves every byte. `TileMap` derives a row-major shared
`TileMapData` view without modifying the file or retaining a second representation.

## Creation and conversion

```csharp
var map = new TileMapData(2, 1,
    [new TileMapEntry(42, 3) { MirrorX = true }, new TileMapEntry(256)]);
var file = new NxmFile(map, new NxmFormat(2, 1, NxmEncoding.Tiles512));

var target = new NxmFormat(2, 1, NxmEncoding.Tiles512, ResourceDataOrder.ColumnMajor);
var converted = new TileMapToNxmConverter(file.Format, target).Convert(file);
```

[`TileMapToNxmConverter`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm/TileMapToNxmConverter/index.md)
is explicit, not globally registered: the target file type alone cannot determine its dimensions or encoding.
It supports reordering and range-checked index-width changes, and creates independent output components.
Wrong dimensions, overflowing indices/palette offsets and unsupported flags throw `ArgumentException`.
There is no silent masking, attribute removal, resizing or automatic tile/block remapping.

Palette offsets are numeric selectors in the chosen encoding: graphics selects sixteen-colour groups,
text selects two-colour pairs. Conversion preserves selectors, not rendered colour equivalence between
different palette interpretations. Maps do not own palettes or referenced tile/block collections, so they
cannot validate those assets or render an image on their own. No image-to-map converter is invented here;
tile extraction/deduplication is a separate operation. CLI and online-converter integration remain deferred.
