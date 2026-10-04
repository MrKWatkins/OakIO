# NXB

`MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb` reads and writes Gfx2Next's headerless tile-block
collections. Blocks are reusable rectangular groups of **tile indices**, not pixel data. An
[NXM](../tilemap/nxm.md) map can refer to the blocks, which in turn refer to tiles stored separately in
[NXT](../tiles/nxt.md).

References: [Gfx2Next options](https://github.com/benbaker76/Gfx2Next) and its
[pinned implementation](https://github.com/benbaker76/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c)
(`write_blocks` and `get_block`). The implementation reference is also linked in code comments.

## Layout

There is no header, palette, embedded graphics or stored block count. Blocks occur consecutively;
each block stores its entries left to right within each row, then top to bottom. Dimensions count
tile entries, not pixels. Width, height and index width must be supplied externally.

| Index width | Bytes per entry | Index range |
| --- | --- | --- |
| 8 bits | 1 | 0–255 |
| 16 bits | 2, little-endian | 0–65535 |

These are plain indices, **not** NXM's attribute-bearing tile/attribute pairs. In the inspected Gfx2Next
writer, `get_block` stores the tile number but does not retain its palette, mirror or rotation attributes.
OakIO neither invents an attribute-bearing NXB variant nor silently discards attributes during conversion.

File length must be a multiple of `width * height * bitsPerIndex / 8`. The count is derived from length;
empty collections are valid, partial blocks throw `InvalidDataException`. There is no hardware-slot count
limit or automatic deduplication. Headerless files cannot reveal whether the supplied interpretation
matches their original export settings. Custom traversal orders are outside this format.
Existing OakIO compression wrappers remain available.

## Reading and writing

```csharp
var format = new NxbFormat(3, 2, bitsPerIndex: 16);
NxbFile file = format.Read(bytes);
byte[] original = file.ToByteArray();
await file.WriteAsync(outputStream);

ResourceFormat[] formats = [.. ZXSpectrumNextResourceFileFormats.AllFormats, format];
var loaded = await IOFileFormat.LoadAsync("blocks.nxb", inputStream, formats);
```

[`NxbFormat`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb/NxbFormat/index.md) supports
synchronous/asynchronous and compressed APIs. Unqualified resource lists exclude NXB because they
cannot supply its layout; add one explicitly selected format to discovery.

[`NxbFile`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb/NxbFile/index.md) owns an ordered
`Entries` collection of byte-backed
[`NxbBlock`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb/NxbBlock/index.md) components.
Each component retains native bytes in `Data` and its interpretation in `Format`. Input bytes are copied
and reads/writes preserve every byte. `Blocks` derives a shared `TileBlocksData`; each component's
`TileMap` derives its row-major map view. These views are not retained second representations.

## Creation and conversion

```csharp
var blocks = new TileBlocksData(2, 1,
    [new TileMapEntry(0), new TileMapEntry(255),
     new TileMapEntry(256), new TileMapEntry(65535)]);
var file = new NxbFile(blocks, new NxbFormat(2, 1, 16));

TileMapData secondBlock = file.Blocks.GetBlock(1);
var mapFile = new NxmFile(secondBlock, new NxmFormat(2, 1, NxmEncoding.Index16));
```

[`TileBlocksToNxbConverter`](../../API/MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb/TileBlocksToNxbConverter/index.md)
converts another block file using an explicit target layout. It is not globally registered: the NXB file
type alone cannot determine block dimensions or index width.

```csharp
var converter = new TileBlocksToNxbConverter(source.Format, new NxbFormat(3, 2, 16));
NxbFile converted = converter.Convert(source);
```

Conversions preserve block order, duplicates and indices, and create independent components. Widening
8-bit indices is supported; narrowing requires every index to fit. Wrong dimensions, out-of-range indices
and any palette/mirror/rotation/priority attribute throw `ArgumentException`, even when the output uses
16-bit indices. No masking, block reshaping, tile reindexing or implicit attribute removal occurs.

The format does not own referenced NXT graphics, so it cannot validate references against a tiles file or
render images without additional assets. Map expansion and block extraction/deduplication are separate
operations, not implicit file conversions. CLI and online-converter integration remain deferred.
