# GIMP GPL

GIMP palettes (`.gpl`) store decimal RGB components with optional palette and colour names. Alpha is not supported.

Details about the format can be found in the [GIMP GPL specification](https://developer.gimp.org/core/standards/gpl/).

## API

| Class | Description |
| --- | --- |
| [`GplFormat`](../../API/MrKWatkins.OakIO.Resources.Gpl/GplFormat/index.md) | Singleton format for reading and writing palettes. |
| [`GplFile`](../../API/MrKWatkins.OakIO.Resources.Gpl/GplFile/index.md) | Palette file with typed components. |
| [`GplHeader`](../../API/MrKWatkins.OakIO.Resources.Gpl/GplHeader/index.md) | Byte-backed header and format metadata. |
| [`GplColourEntry`](../../API/MrKWatkins.OakIO.Resources.Gpl/GplColourEntry/index.md) | Byte-backed colour entry. |

## Reading and Writing

```c#
using var input = File.OpenRead("palette.gpl");
GplFile file = GplFormat.Instance.Read(input);

using var output = File.Create("output.gpl");
file.Write(output);
```

Loading and saving preserves the original UTF-8 bytes, optional BOM, line endings and whitespace. Comments, ordering and duplicate colours are retained. Invalid UTF-8 and palettes without colour entries are rejected. The format also supports asynchronous I/O and compression; see [Reading, Writing and Converting](../../reading-writing-converting.md).

## Components

GPL files contain a byte-backed `Header` and ordered body `Lines`. The header exposes `Signature`, nullable `Name`, `HasColumns` and `Columns` (0–255). Older headers without a name and newer headers with a name and optional columns are supported. A missing name is `null`; an explicitly empty name is an empty string.

Colour entries expose `Colour` and `Name`; unnamed entries have an empty name. Lines starting with `#` are comments.

`Entries` exposes colour entries in palette order. Body lines expose their original `Data`, decoded `Text` and `LineEnding`. `Palette` derives a shared `PaletteData` view from the entries rather than storing an independent copy.

## Creating Palettes

```c#
var palette = new PaletteData([new Colour(255, 0, 0), new Colour(0, 255, 0)]);
var file = new GplFile(palette, name: "Sprites", columns: 2, colourNames: ["Red", "Green"]);
byte[] bytes = file.ToByteArray();
```

Constructors produce canonical UTF-8 without a BOM, using LF line endings. GPL creation defaults to palette name `OakIO` and automatic columns (zero). Supplied colour names must match the colour count, and names cannot contain line endings. Non-opaque colours are rejected with `ArgumentException`.

## Conversions

```c#
JascFile jasc = IOFileConversion.Convert<JascFile>(file);
PaintNetFile paintNet = IOFileConversion.Convert<PaintNetFile>(file);
```

Conversions preserve colour values, order and duplicates without quantization. Comments and source-format metadata are not transferred; GPL output uses its constructor defaults. Conversion to Paint.NET produces opaque entries. See [Reading, Writing and Converting](../../reading-writing-converting.md) for the conversion API.
