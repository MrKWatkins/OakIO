# SCR

`MrKWatkins.OakIO.ZXSpectrum.Resources.Scr` in the Spectrum package reads and writes standard
256×192 Spectrum screen dumps. A file is exactly 6912 bytes, with no header, palette or border colour:

| Offset | Bytes | Component |
| --- | --- | --- |
| 0 | 6144 | Bitmap in native screen-memory order |
| 6144 | 768 | Attributes for 32×24 cells, in row-major order |

The bitmap has three 64-line thirds. Within each third it stores scan zero of the eight character rows,
then scan one of those rows, continuing in sequence. Each byte selects ink or paper for 8 pixels, with bit seven
on the left. Attributes are `FBPPPIII`: FLASH, BRIGHT, three paper bits and three ink bits.

References: [Spectrum FAQ — SCR file layout](https://worldofspectrum.org/faq/reference/formats.htm),
[Sinclair manual — screen-memory layout](https://worldofspectrum.org/ZXBasicManual/zxmanchap24.html),
and [Sinclair manual — colour attributes](https://worldofspectrum.org/ZXBasicManual/zxmanchap16.html).
These references are also linked in the implementation comments.

## Reading and writing

```csharp
ScrFile file = ScrFormat.Instance.Read(bytes);
byte[] original = file.ToByteArray();
await file.WriteAsync(outputStream);

var loaded = await IOFileFormat.LoadAsync("screen.scr", inputStream,
    ZXSpectrumResourceFileFormats.AllFormats);
```

[`ScrFormat`](../../API/MrKWatkins.OakIO.ZXSpectrum.Resources.Scr/ScrFormat/index.md) supports the usual
synchronous, asynchronous and compressed APIs. `ZXSpectrumResourceFileFormats.AllFormats` includes
general resources and SCR; the Next resource list includes these Spectrum resources too. Snapshot/tape
lists and core-only `ResourceFileFormats.AllFormats` remain separate.

Nonstandard lengths throw `InvalidDataException`. Extended Timex and ULAplus SCR variants are not supported,
and trailing bytes are not silently discarded. Empty files are invalid.

## Components and image view

[`ScrFile`](../../API/MrKWatkins.OakIO.ZXSpectrum.Resources.Scr/ScrFile/index.md) owns two byte-backed components:

- [`ScrBitmap`](../../API/MrKWatkins.OakIO.ZXSpectrum.Resources.Scr/ScrBitmap/index.md): `Data` retains native
  bitmap bytes. `GetPixel(x, y)` returns the ink-selection bit using top-left-origin pixel coordinates.
- [`ScrAttributes`](../../API/MrKWatkins.OakIO.ZXSpectrum.Resources.Scr/ScrAttributes/index.md): `Data` retains
  all attribute bytes. `GetAttribute(x, y)` uses **cell** coordinates (32×24), returning a
  [`ScrAttribute`](../../API/MrKWatkins.OakIO.ZXSpectrum.Resources.Scr/ScrAttribute/index.md) with `Value`,
  `Ink`, `Paper`, `Bright`, `Flash`, `InkColour` and `PaperColour`.

Writes preserve all bytes, including FLASH and attributes whose ink and paper are identical. Inputs are copied.
The file retains neither artificial headers nor independent normalised pixel arrays.

`Image` derives an `IndexedImageData` in top-left row-major order, with unpacked indices into a 16-entry
rendering palette. Entries 0–7 are normal black, blue, red, magenta, green, cyan, yellow and white;
entries 8–15 are their bright variants. Both black entries are retained, even though their RGB colours are equal.
The shared view uses four-bit indices, **not** the bitmap's one-bit on-disk packing.

Normal RGB components are `0xC0`, bright components `0xFF`, following
[OakEmu's colour conversion](https://github.com/MrKWatkins/OakEmu/blob/main/src/MrKWatkins.OakEmu.Machines.ZXSpectrum/Screen/PixelColourExtensions.cs).
These are a rendering convention, not a claim about the analogue output of every Spectrum.
The scalar and SIMD decoders are adapted from
[OakEmu's screen converters](https://github.com/MrKWatkins/OakEmu/tree/main/src/MrKWatkins.OakEmu.Machines.ZXSpectrum/Screen).
The decoder selects SIMD when `Vector256` is hardware accelerated, otherwise the scalar fallback.

```csharp
IndexedImageData ordinary = file.Render();
IndexedImageData alternate = file.Render(flashPhase: true);
```

The alternate phase exchanges ink and paper **only** for FLASH cells. Neither phase changes the file.
`Image` uses the ordinary phase; there is no timer or automatic animation.

## Creation and conversion

Create a file from 6912 raw bytes with `new ScrFile(bytes)`, or from an `ImageData` with
`new ScrFile(image)`. Image creation requires exactly 256×192 pixels and opaque colours from the rendering
palette. Each 8×8 cell may use at most two distinct colours, with a shared brightness level; black can accompany
either normal or bright colours. Different cells may use different levels.

Unsupported colours, transparency, more than two colours per cell or mixed normal/bright non-black colours
throw `ArgumentException`. Conversion performs no implicit cropping, scaling, quantization or dithering.
New attributes have FLASH cleared. Ink/paper assignments and bitmap bits are chosen deterministically from
the pixels, but converting an image back need not reproduce the original SCR bytes or its FLASH settings.

[`ImageToScrConverter`](../../API/MrKWatkins.OakIO.ZXSpectrum.Resources.Scr/ImageToScrConverter/index.md)
works with image formats explicitly and honours palette colours, not numeric source indices:

```csharp
var converter = new ImageToScrConverter(source.Format);
ScrFile screen = converter.Convert(source);
```

BMP→SCR, SCR→BMP and SCR→SCR are registered with `IOFileConversion` when the Spectrum assembly loads:

```csharp
ScrFile screen = IOFileConversion.Convert<ScrFile>(bmp);
BmpFile bitmap = IOFileConversion.Convert<BmpFile>(screen);
```

SCR→SCR creates an independent byte-for-byte copy, preserving FLASH. SCR→BMP exports the ordinary image
phase; BMP has no Spectrum attribute/FLASH metadata, so animation is lost on that conversion.
CLI and online-converter integration remain deferred until the resource stages are complete.
