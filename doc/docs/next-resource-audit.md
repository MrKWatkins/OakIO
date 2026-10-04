# Next resource format audit

Audited 2026-10-04 against the current Gfx2Next writer at
[e3a6abcd](https://github.com/RustyPixelsUK/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c),
[zxnext_bmp_tools at 3b0b44ff](https://github.com/stefanbylund/zxnext_bmp_tools/blob/3b0b44ff9536c38d4164531c921292c4be316076/src/nextraw.c),
[showsimg at c79616ac](https://gitlab.com/varmfskii/showsimg/-/blob/c79616ac578fb9372c2b34a413f091290b549959/FORMAT.md)
and the [Next file-format catalogue](https://wiki.specnext.dev/File_Formats).
These are exporter/loader conventions, not a universal schema for every file bearing an extension.

| Family | Implemented interpretation | Other observed variants / limitations |
|---|---|---|
| [NXP](formats/palette/nxp.md) | 16/256 two-byte RGB333 entries; no priority flags | Gfx2Next RGB332, GBR444 and BGR222 encodings need explicit additional models. Full 256-entry RGB333 palettes are supported as NXP, including for four-bit assets, but the strict four-bit NXI model embeds only 16 entries. Never infer colour space merely from length. |
| [SPR](formats/tiles/spr.md) | 16×16, chunky 4/8-bit, explicit depth, arbitrary count | Font/custom-size exports, embedded palettes and custom data order are not interpreted. Hardware slot count is not an asset-file limit. |
| [NXT](formats/tiles/nxt.md) | 8×8 chunky 1/4/8-bit, explicit depth | Exporter custom dimensions, planar/column-major data and embedded palettes require additional explicit layout options. |
| [NXM](formats/tilemap/nxm.md) | Explicit dimensions/order/plain indices or selected Next attributes | Can represent tile or block references numerically; does not automatically resolve referenced graphics, render maps or expand NXB blocks. Other platform-specific exporter attributes are outside the Next codecs. |
| [NXB](formats/blocks/nxb.md) | Explicit block dimensions, row-major plain byte/word indices, block-major collection | No attributes in the native Gfx2Next block writer. Duplicate blocks remain intact; no reindexing/deduplication or implicit map expansion. |
| [NXI](formats/image/nxi.md) | Native Layer 2 modes with prepended RGB333 palette (16/256 entries) | Exporters also produce palette-free/separate, RGB332, full 256-entry four-bit palettes, arbitrary sizes/order, bank splitting/padding and per-section compression. Current NxiFormat does not guess these. |
| [SL2](formats/image/sl2.md) | Three native modes, appended absent/RGB332/RGB333 palette; optional +3DOS header | Explicit mode needed for identical wide raw sizes. Standard NextZXOS browser variant is header + 256×192 bitmap without palette; loader support for extensions differs. |
| [SLR](formats/image/slr.md) | 128×96 LoRes 8-bit or packed Radastan 4-bit; appended optional palette/header | No video RAM gap in file; bank choice and hardware palette offset are external. |
| [SCR](formats/image/scr.md) | Standard 6912-byte Spectrum bitmap/attributes | +3DOS-wrapped, ULAplus/ULANext palette extensions and related SHC/SHR/MC/MLT Timex dumps need separate support. These are Spectrum-family formats, not silently folded into SCR. |

## Additional named files

The Next catalogue also lists native binary `.pal` (256 RGB333 entries with priority) and
`.npl` (that palette plus a transparency byte). They are **not yet implemented** as distinct
native formats. Core `.pal` remains strictly JASC-PAL as agreed; a future native palette
format must be explicitly selected rather than changing that meaning by length.
NXP is not a lossless replacement for priority/transparency metadata.

VID animation/video, executable DOT/BAS and SNX snapshots are different resource/executable
families and remain outside this image/tile/palette batch. NEX support already exists.
TMX/TSX remains explicitly excluded. CLI/WASM resource UI integration is still deferred.

## Follow-up order

1. Native PAL/NPL metadata and explicitly selectable palette encodings.
2. Explicit NXI variants, reusing the new screen palette component where suitable.
3. Explicit tile dimensions/order/planar and embedded-palette variants.
4. Exporter-specific bank chunks only with concrete use cases.

This audit does not claim all Next/exporter variants are supported. Reuse typed base resource
models, preserve native metadata and require caller-selected interpretation for ambiguous
headerless formats. Project resource wiring can initially use the implemented subset and
give diagnostics for unsupported variants.
