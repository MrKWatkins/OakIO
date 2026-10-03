# Resources

The resource types are in the core `MrKWatkins.OakIO` package, under the
`MrKWatkins.OakIO.Resources` namespace. They provide a common data model for image, palette and tile formats.
Concrete formats include [BMP](formats/image/bmp.md), [GIMP GPL](formats/palette/gpl.md),
[JASC PAL](formats/palette/jasc.md) and [Paint.NET](formats/palette/paintdotnet.md) palettes.
The `MrKWatkins.OakIO.ZXSpectrumNext` package also supports [NXP](formats/palette/nxp.md) palettes.
It also provides [SPR](formats/tiles/spr.md) sprite patterns and [NXT](formats/tiles/nxt.md) tiles.
Resource formats are not yet included in the CLI or online converter.

## Files and formats

[`ResourceFile`](API/MrKWatkins.OakIO.Resources/ResourceFile/index.md) inherits from `IOFile`, so resource files
use the existing synchronous and asynchronous writing, compression and conversion infrastructure.
[`ResourceFormat`](API/MrKWatkins.OakIO.Resources/ResourceFormat/index.md) inherits from `IOFileFormat`.
Each specialized pair has the same relationship:

| File | Format | Shared data |
| --- | --- | --- |
| [`ImageFile`](API/MrKWatkins.OakIO.Resources/ImageFile/index.md) | `ImageFormat` | `ImageData` |
| [`PaletteFile`](API/MrKWatkins.OakIO.Resources/PaletteFile/index.md) | `PaletteFormat` | `PaletteData` |
| [`TilesFile`](API/MrKWatkins.OakIO.Resources/TilesFile/index.md) | `TilesFormat` | `TilesData` |

Each format also has a generic variant for strongly typed reads and writes. A concrete format implements its
binary reader and writer hooks and registers applicable converters through `CreateConverters`.
Concrete resource files retain their own format-specific headers and data components, following the tape
and snapshot formats. The image, palette and tiles bases take only a format; their abstract `Image`,
`Palette` and `Tiles` properties provide convenience views implemented by each concrete file. Shared data
models are conversion views, not a replacement for the on-disk structure or a second source of truth.
Resource formats are separate from the Spectrum and Next snapshot/tape format lists. Call
`IOFileFormat.Load` or `LoadAsync` with `ResourceFileFormats.AllFormats` to discover resource formats.
For general and Next resource formats together, use `ZXSpectrumNextResourceFileFormats.AllFormats`
from the Next package's `Resources` namespace.
SPR and NXT require explicit bit depths and are excluded from unqualified discovery. Use
`ZXSpectrumNextResourceFileFormats.WithTiles(spriteBitsPerPixel, tileBitsPerPixel)` to include them.

## Colours and palettes

[`Colour`](API/MrKWatkins.OakIO.Resources/Colour/index.md) stores eight-bit red, green, blue and alpha components.
The three-component constructor defaults alpha to 255 (opaque); zero alpha is transparent.
Quantization to hardware colour depths belongs to the target-format converter, not this shared type.

[`PaletteData`](API/MrKWatkins.OakIO.Resources/PaletteData/index.md) contains one or more ordered colours.
Order and duplicates are preserved because an index identifies a specific entry. The general palette model
does not impose a hardware size limit; concrete formats and indexed images impose their own limits.

## Images

[`ImageData`](API/MrKWatkins.OakIO.Resources/ImageData/index.md) has positive width and height, a pixel count
and `GetPixel(x, y)`. The origin is at the top-left. Pixels are row-major: left to right within each row,
then top to bottom.

- [`ColourImageData`](API/MrKWatkins.OakIO.Resources/ColourImageData/index.md) stores one `Colour` per pixel.
- [`IndexedImageData`](API/MrKWatkins.OakIO.Resources/IndexedImageData/index.md) stores one byte per palette
  index and references a reusable `PaletteData`. Its index bit depth is 1, 2, 4 or 8.

Indexed pixels are always unpacked in memory, even for four-bit images. Every index must identify an existing
palette entry, and the palette must fit the bit depth. Disk-specific packing, row padding and row/column order
are handled by the concrete file format.

```c#
var palette = new PaletteData([new Colour(0, 0, 0), new Colour(255, 0, 0)]);
var image = new IndexedImageData(2, 2, [0, 1, 1, 0], palette, bitsPerPixel: 1);
Colour red = image.GetPixel(1, 0);
```

## Tiles

[`TilesData`](API/MrKWatkins.OakIO.Resources/TilesData/index.md) stores equally sized indexed tiles.
`Width` and `Height` describe each tile; `Count` is calculated from the pixel data. An empty tile collection
is allowed, but partial tiles are not.

Pixels are tile-major: all of tile zero, then all of tile one, and so on. Within each tile, indices are row-major
and unpacked, with a bit depth of 1, 2, 4 or 8. Tiles do not own a palette, so a palette can be supplied separately
and shared between images or tiles.

## Ownership and validation

The concrete shared data models copy input sequences and expose read-only collections. A shared palette can
therefore be safely reused without later input mutations changing the image colours. Invalid dimensions,
pixel counts, bit depths and indices are rejected when the data model is constructed.
