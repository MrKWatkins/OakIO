# SL2 — Next Layer 2 screens

SL2 stores native bitmap bytes followed by an optional palette. This differs from
[NXI](nxi.md), which prepends the palette. An optional 128-byte +3DOS header is preserved.

| Explicit mode | Bitmap bytes | Storage order | Optional palette bytes |
|---|---:|---|---:|
| 256×192, 8-bit | 49152 | Row-major | 256 RGB332 / 512 RGB333 |
| 320×256, 8-bit | 81920 | Column-major | 256 RGB332 / 512 RGB333 |
| 640×256, 4-bit | 81920 | Column-major packed pixel pairs | 16 RGB332 / 32 RGB333 |

The high nibble is the left pixel. Width, height and depth describe the selected native
layout; they are not fields invented in a header. Palette-free 320/640 files have identical
lengths, so select the mode explicitly. No dimensions or bit depth are guessed.

## Reading and writing

Use [Sl2Format](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Sl2/Sl2Format/index.md):
`Standard`, `Wide`, or `HighResolution`; `ForDimensions` selects the corresponding singleton.

```csharp
var screen = Sl2Format.Wide.Read(bytes);
var image = screen.Image; // Unpacked, top-left-origin, row-major indexed view.
var header = screen.Header; // Null for a headerless file.
screen.Write(stream); // Preserves native bytes, including header metadata and priority bits.
```

[Sl2File](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Sl2/Sl2File/index.md)
exposes byte-backed `Header`, `PixelData` and `Palette`.
[Plus3DosHeader](../../API/MrKWatkins.OakIO.ZXSpectrum.Plus3Dos/Plus3DosHeader/index.md)
exposes issue/version, complete size, BASIC type/length/parameters and checksum.
The header signature, soft EOF, checksum and complete file length are validated.
Reserved bytes and other metadata survive reading/writing. An exact raw layout takes
precedence if its pixel bytes happen to start with `PLUS3DOS`.

## Palettes and conversion

[ScreenPaletteEncoding](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens/ScreenPaletteEncoding/index.md)
selects no palette, RGB332 or RGB333. RGB333's second byte permits the low blue bit
and the Layer 2 priority bit; other bits must be zero.
[ScreenPalette](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens/ScreenPalette/index.md)
exposes the encoding, derived colours and `GetPriority`.

Without stored palette bytes, the convenience image uses the default RGB332 palette:
the missing blue bit is the OR of the two stored blue bits. This is an interpretation,
not a claim about the palette currently loaded on a running machine. Supply an embedded
palette when those colours differ. Creation without palette bytes requires that default
palette; it never silently drops arbitrary colours.

```csharp
var screen = new Sl2File(indexedImage, Sl2Format.Standard,
    ScreenPaletteEncoding.Rgb333, includeHeader: true);
var converter = new ImageToSl2Converter(BmpFormat.Instance, Sl2Format.Standard,
    ScreenPaletteEncoding.Rgb333, includeHeader: true);
var converted = converter.Convert(bmp);
```

Creation requires matching dimensions, fitting indices and exactly 16/256 opaque colours.
Palette order and duplicate slots remain intact; colours are rounded to the nearest
representable hardware levels. No implicit quantization, reindexing, resizing or palette
generation occurs. Explicit conversion re-creates the requested layout and does not preserve
source header metadata or palette priority flags; use native read/write for lossless copies.
Screen-to-BMP conversion is registered; incoming converters are explicit because target type
alone cannot choose mode/palette/header options.

Use `ZXSpectrumNextResourceFileFormats.WithScreens(layer2, lowResolution)` for discovery;
the unqualified list excludes SL2/SLR. Existing OakIO compression wrappers can wrap the whole file.
Truncated bitmaps, partial/unrecognized palette lengths, unsupported modes and invalid headers
are rejected. Header/palette combinations extend the browser's basic no-palette screen variant;
not every loader accepts every variant. No arbitrary-size dumps or bank padding are guessed.

## References

- [showsimg format definitions](https://gitlab.com/varmfskii/showsimg/-/blob/c79616ac578fb9372c2b34a413f091290b549959/FORMAT.md)
- [Next file formats](https://wiki.specnext.dev/File_Formats)
- [Layer 2 native memory layout](https://wiki.specnext.dev/Layer_2)
- [Next palette interpretation](https://wiki.specnext.dev/Palettes)
- [+3DOS header specification](https://worldofspectrum.org/ZXSpectrum128%2B3Manual/chapter8pt27.html)
