# SLR — Next low-resolution screens

SLR stores a 128×96 bitmap followed by an optional palette, with an optional preserved
128-byte +3DOS header. Pixel data is row-major without Spectrum bitmap interleaving.

| Explicit mode | Bitmap bytes | Packing | Optional palette bytes |
|---|---:|---|---:|
| 8-bit LoRes | 12288 | One byte per pixel | 256 RGB332 / 512 RGB333 |
| 4-bit Radastan | 6144 | Left pixel in high nibble | 16 RGB332 / 32 RGB333 |

The 8-bit hardware image occupies two 6144-byte regions separated in video RAM; the file
contains them consecutively, not the intervening memory gap. Four-bit mode contains one
packed bitmap; hardware palette offset/bank selection is not stored in the file.

## API

[SlrFormat](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Slr/SlrFormat/index.md)
provides `EightBit`, `FourBit` and `ForBitsPerPixel`. Select one explicitly.
[SlrFile](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Slr/SlrFile/index.md)
exposes the native header, pixel and palette components and a derived indexed image.

```csharp
var screen = SlrFormat.EightBit.Read(bytes);
var created = new SlrFile(indexedImage, SlrFormat.EightBit,
    ScreenPaletteEncoding.Rgb333, includeHeader: false);
var converter = new ImageToSlrConverter(BmpFormat.Instance, SlrFormat.EightBit);
var converted = converter.Convert(bmp);
```

Palette/header validation, colour rounding, default-palette interpretation, lossless native
IO and explicit-conversion limitations are the same as [SL2](sl2.md).
Conversion requires exactly 128×96 indexed pixels and the full native palette, without
resizing, reindexing, dithering or automatic palette generation.
Incoming conversion is explicit; screen-to-BMP is registered.
Use `ZXSpectrumNextResourceFileFormats.WithScreens(layer2, lowResolution)` for discovery.
Existing OakIO compression wrappers can wrap the whole file.

## References

- [showsimg format definitions](https://gitlab.com/varmfskii/showsimg/-/blob/c79616ac578fb9372c2b34a413f091290b549959/FORMAT.md)
- [LoRes and Radastan hardware layouts](https://wiki.specnext.dev/Video_Modes)
- [Next file formats](https://wiki.specnext.dev/File_Formats)
- [Next palettes](https://wiki.specnext.dev/Palettes)
- [+3DOS header specification](https://worldofspectrum.org/ZXSpectrum128%2B3Manual/chapter8pt27.html)
