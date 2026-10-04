# BMP

`MrKWatkins.OakIO.Resources.Bmp` in the core package supports reading and writing uncompressed Windows
BMP files with a 40-byte `BITMAPINFOHEADER`, at four/eight-bit indexed or 24-bit RGB depth.
Other header sizes, depths and compression methods throw `NotSupportedException`.
Malformed headers and indices throw `InvalidDataException`; truncated data throws `EndOfStreamException`.
Dimensions and pixel bounds are checked before allocating image arrays.

Rows can be stored bottom-up or top-down and are padded to four-byte boundaries. The shared image model
always uses top-left row-major order. Indexed pixels are packed high-nibble first at four-bit depth;
palette order and duplicate colours are preserved. The palette's fourth byte is reserved, not alpha.
See Microsoft's [bitmap header](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader),
[file header](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapfileheader) and
[RGBQUAD](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-rgbquad) documentation.

```csharp
var image = new ColourImageData(2, 1, [new Colour(255, 0, 0), new Colour(0, 255, 0)]);
var file = new BmpFile(image, isTopDown: true);
byte[] bytes = file.ToByteArray();
BmpFile loaded = BmpFormat.Instance.Read(bytes);
```

New files default to bottom-up storage and have canonical headers and zero padding. Loaded files retain
their original bytes: headers, palette reserved bytes, packed pixel rows and padding, gaps before the
pixel data, and trailing data (including bytes beyond the declared file size). Writing emits these
components unchanged rather than reconstructing them from a normalised image.

## File components and metadata

`BmpFile.Header` is a byte-backed `BmpHeader` exposing the signature, declared file size, both reserved
fields and pixel-data offset. `InformationHeader` is a `BmpInformationHeader` exposing every field of the
40-byte `BITMAPINFOHEADER`: width, signed height, planes, bit depth, compression, image size, horizontal
and vertical resolution in pixels per metre, used colours and important colours. Convenience properties
provide row stride and top-down orientation without altering the stored fields.

`Palette` is a `BmpPalette`, or null if no colour table is present. `Data` retains the original RGBQUAD
bytes; `Colours` exposes opaque RGB colours. Explicit RGB24 optimization colour tables are retained too.
`PixelData` is a `BmpPixelData`: its `Data` retains packed rows in their stored orientation, with padding.
These components use OakIO's existing `Header` and `IOFileComponent` bases. BMP has no separate per-row
header/trailer, so it does not introduce artificial block headers.

`Image` is a normalised convenience view derived from these components, not independently stored data.
It is reconstructed when accessed. `Gap` and `TrailingData` expose the remaining retained bytes.

Alpha is not supported when creating images: all pixels and palette colours must be opaque,
including unused palette entries.

`ImageToBmpConverter` preserves RGB data and four/eight-bit indices, promotes one/two-bit indexed data
to eight bits without changing the palette or indices, and rasterises other `ImageData` implementations.
It does not quantize colours. Source formats can register it in `CreateConverters` to participate in
`IOFileConversion`; target-specific converters will be added with their corresponding formats.

Use `ResourceFileFormats.AllFormats` with `IOFileFormat.Load`/`LoadAsync` for filename-based discovery.
The usual synchronous/asynchronous and compressed file APIs are available. CLI and online converter
integration is deferred until the resource format stages are complete.
