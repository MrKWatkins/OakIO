# Paint.NET

Paint.NET palettes (`.txt`) contain eight-digit hexadecimal colour entries in `AARRGGBB` order, including alpha.

Details about the format can be found in the [Paint.NET palette documentation](https://paint.net/doc/latest/WorkingWithPalettes.html).

## API

| Class | Description |
| --- | --- |
| [`PaintNetFormat`](../../API/MrKWatkins.OakIO.Resources.PaintNet/PaintNetFormat/index.md) | Singleton format for reading and writing palettes. |
| [`PaintNetFile`](../../API/MrKWatkins.OakIO.Resources.PaintNet/PaintNetFile/index.md) | Palette file with typed components. |
| [`PaintNetColourEntry`](../../API/MrKWatkins.OakIO.Resources.PaintNet/PaintNetColourEntry/index.md) | Byte-backed colour entry. |

## Reading and Writing

```c#
using var input = File.OpenRead("palette.txt");
PaintNetFile file = PaintNetFormat.Instance.Read(input);

using var output = File.Create("output.txt");
file.Write(output);
```

Loading and saving preserves the original UTF-8 bytes, optional BOM, line endings and whitespace. Comments, ordering and duplicate colours are retained. Invalid UTF-8 and palettes without colour entries are rejected. The format also supports asynchronous I/O and compression; see [Reading, Writing and Converting](../../reading-writing-converting.md).

## Components

Paint.NET has no required header. The ordered `Lines` collection retains every line, with colour lines represented by `PaintNetColourEntry`. Each entry exposes its decoded `Colour`, including alpha. Lines starting with `;` are comments.

The model retains all actual entries without padding or truncation. Paint.NET's UI displays 96 slots, filling missing slots with white and ignoring additional colours; this UI behaviour is not applied to stored file data.

`Entries` exposes colour entries in palette order. Body lines expose their original `Data`, decoded `Text` and `LineEnding`. `Palette` derives a shared `PaletteData` view from the entries rather than storing an independent copy.

## Creating Palettes

```c#
var palette = new PaletteData([new Colour(255, 0, 0), new Colour(0, 255, 0)]);
var file = new PaintNetFile(palette);
byte[] bytes = file.ToByteArray();
```

Constructors produce canonical UTF-8 without a BOM, using LF line endings. Creation retains alpha and writes uppercase hexadecimal entries, preceded by a `; Paint.NET Palette File` comment.

## Conversions

```c#
GplFile gpl = IOFileConversion.Convert<GplFile>(file);
JascFile jasc = IOFileConversion.Convert<JascFile>(file);
```

Conversions preserve colour values, order and duplicates without quantization. Comments and source-format metadata are not transferred; GPL output uses its constructor defaults. Conversion to GPL or JASC rejects non-opaque colours with `ArgumentException` rather than losing alpha. See [Reading, Writing and Converting](../../reading-writing-converting.md) for the conversion API.
