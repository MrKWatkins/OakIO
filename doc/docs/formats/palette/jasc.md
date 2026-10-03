# JASC PAL

JASC Paint Shop Pro palettes (`.pal`) store decimal RGB components. Only the JASC variant is supported; other formats sharing the `.pal` extension are not inferred.

Details about the format can be found in the [GIMP palette format reference](https://developer.gimp.org/core/standards/).

## API

| Class | Description |
| --- | --- |
| [`JascFormat`](../../API/MrKWatkins.OakIO.Resources.Jasc/JascFormat/index.md) | Singleton format for reading and writing palettes. |
| [`JascFile`](../../API/MrKWatkins.OakIO.Resources.Jasc/JascFile/index.md) | Palette file with typed components. |
| [`JascHeader`](../../API/MrKWatkins.OakIO.Resources.Jasc/JascHeader/index.md) | Byte-backed header and format metadata. |
| [`JascColourEntry`](../../API/MrKWatkins.OakIO.Resources.Jasc/JascColourEntry/index.md) | Byte-backed colour entry. |

## Reading and Writing

```c#
using var input = File.OpenRead("palette.pal");
JascFile file = JascFormat.Instance.Read(input);

using var output = File.Create("output.pal");
file.Write(output);
```

Loading and saving preserves the original UTF-8 bytes, optional BOM, line endings and whitespace. Comments, ordering and duplicate colours are retained. Invalid UTF-8 and palettes without colour entries are rejected. The format also supports asynchronous I/O and compression; see [Reading, Writing and Converting](../../reading-writing-converting.md).

## Components

JASC files contain a byte-backed `Header` and ordered body `Lines`. The header exposes `Signature`, `Version` and `ColourCount`. The required signature is `JASC-PAL`, and the supported version is `0100`.

The positive colour count must match the actual entries. Each colour entry contains exactly three decimal byte components and exposes `Colour`; names and alpha are not supported. Blank body lines are retained. Invalid signatures, counts and entries throw `InvalidDataException`; unsupported versions throw `NotSupportedException`.

`Entries` exposes colour entries in palette order. Body lines expose their original `Data`, decoded `Text` and `LineEnding`. `Palette` derives a shared `PaletteData` view from the entries rather than storing an independent copy.

## Creating Palettes

```c#
var palette = new PaletteData([new Colour(255, 0, 0), new Colour(0, 255, 0)]);
var file = new JascFile(palette);
byte[] bytes = file.ToByteArray();
```

Constructors produce canonical UTF-8 without a BOM, using LF line endings. Non-opaque colours are rejected with `ArgumentException`.

## Conversions

```c#
GplFile gpl = IOFileConversion.Convert<GplFile>(file);
PaintNetFile paintNet = IOFileConversion.Convert<PaintNetFile>(file);
```

Conversions preserve colour values, order and duplicates without quantization. Comments and source-format metadata are not transferred; GPL output uses its constructor defaults. Conversion to Paint.NET produces opaque entries. See [Reading, Writing and Converting](../../reading-writing-converting.md) for the conversion API.
