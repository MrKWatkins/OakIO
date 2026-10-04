# NXI

`MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi` reads and writes native Layer 2 images with an
embedded RGB333 palette. The exact file length identifies one of three layouts.

| Dimensions | Index bits | Palette bytes | Pixel bytes | Total bytes | Pixel order |
| --- | --- | --- | --- | --- | --- |
| 256×192 | 8 | 512 | 49152 | 49664 | Rows |
| 320×256 | 8 | 512 | 81920 | 82432 | Columns |
| 640×256 | 4 | 32 | 81920 | 81952 | Columns of horizontal pixel pairs |

The palette is first, using the same two-byte encoding as [NXP](../palette/nxp.md).
For 640×256, the high nibble is the left pixel and the low nibble the right pixel. Each column
of packed pairs contains 256 bytes, top to bottom. Coordinates have a top-left origin.

References: [nextraw format and options](https://github.com/stefanbylund/zxnext_bmp_tools#nextraw),
[nextraw implementation](https://github.com/stefanbylund/zxnext_bmp_tools/blob/master/src/nextraw.c),
and [Next Layer 2 memory layouts](https://wiki.specnext.dev/Layer_2).
These references are also linked in the implementation comments.

## Reading and writing

```csharp
NxiFile file = NxiFormat.Instance.Read(bytes);
byte[] original = file.ToByteArray();
await file.WriteAsync(outputStream);
var loaded = await IOFileFormat.LoadAsync("screen.nxi", inputStream,
    ZXSpectrumNextResourceFileFormats.AllFormats);
```

[`NxiFormat`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi/NxiFormat/index.md)
supports the usual synchronous, asynchronous and compressed APIs. Discovery includes NXI only in
the Next resource list, not in core/Spectrum resource or snapshot/tape lists.

Only the palette-prefixed native layouts above are supported. Palette-free files, arbitrary sizes,
alternate pixel orders and separated palettes require external interpretation and are not supported.
Other lengths throw `InvalidDataException`; malformed RGB333 entries are rejected. A headerless file
with a supported length is assumed to use the corresponding native layout: its length cannot prove
which export settings created it.

## Components and image view

[`NxiFile`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi/NxiFile/index.md) owns:

- `Palette`: an `NxpFile` containing the embedded byte-backed entries, preserving palette order and duplicates.
- [`PixelData`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi/NxiPixelData/index.md): native bitmap
  bytes, with `Width`, `Height` and `BitsPerPixel` inferred from the file layout. `Pixels` derives unpacked
  row-major indices without changing those bytes.

`Image` derives an `IndexedImageData` with the same indices and a decoded RGB palette. It is a convenience
view, not a second retained representation. Inputs are copied and raw reads/writes are byte preserving.

## Creation and conversion

```csharp
var file = new NxiFile(indexedImage);
NxiFile screen = IOFileConversion.Convert<NxiFile>(bmp);
BmpFile bitmap = IOFileConversion.Convert<BmpFile>(screen);
```

Creation requires exact native dimensions and exactly 256 palette entries for eight-bit modes or
16 for the four-bit mode. Palette colours must be opaque and are rounded to RGB333 using NXP's rules.
Indices and palette order are preserved; indices outside the selected depth are rejected, not truncated.
Both four-bit and eight-bit source indexed images can produce a four-bit screen when their indices and
palette fit. Conversion performs no implicit cropping, resizing, palette generation, padding, trimming or dithering.

[`ImageToNxiConverter`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi/ImageToNxiConverter/index.md)
converts indexed images explicitly. BMP→NXI, NXI→BMP and NXI→NXI are registered with `IOFileConversion`.
NXI→NXI creates an independent byte-for-byte copy. True-colour input requires a separate palette-generation
step and throws `NotSupportedException`; an SCR's 16-entry image palette also requires explicit preparation
before creating a 256×192 NXI. CLI and online-converter integration remain deferred.
