# Home

[![Build Status](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml/badge.svg)](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/MrKWatkins.OakIO)](https://www.nuget.org/packages/MrKWatkins.OakIO)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MrKWatkins.OakIO)](https://www.nuget.org/packages/MrKWatkins.OakIO)

> A C# library for reading, writing and converting between various emulator data formats.

## Overview

OakIO provides a structured object model for working with emulator file formats. It supports reading and writing files, inspecting their contents, and converting between formats.

## Source Code

Source code is available on GitHub at [https://github.com/MrKWatkins/OakIO](https://github.com/MrKWatkins/OakIO).

## Installation

```
dotnet add package MrKWatkins.OakIO
```

## Online Converter

An online converter using this library is available [here](converter.md).

## Documentation

- [Reading, Writing and Converting](reading-writing-converting.md)

### ZX Spectrum Tape Formats

- [TAP](formats/tap.md) — Simple tape format containing raw data blocks.
- [TZX](formats/tzx.md) — The standard for preserving ZX Spectrum tape recordings.
- [PZX](formats/pzx.md) — An alternative tape format with pulse-level encoding.

### ZX Spectrum Snapshot Formats

- [Z80](formats/z80.md) — Snapshot format supporting versions 1, 2, and 3, with optional compression.
- [SNA](formats/sna.md) — Simple snapshot format for 48K and 128K machines.
- [NEX](formats/nex.md) — Snapshot format for the ZX Spectrum Next.

## Architecture

The library is organised around a set of base classes:

- [**IOFileFormat**](API/MrKWatkins.OakIO/IOFileFormat/index.md) — Base class for file formats, providing read and write operations.
- [**IOFile**](API/MrKWatkins.OakIO/IOFile/index.md) — Base class for a file of a given format.
- [**Block**](API/MrKWatkins.OakIO/Block-THeader-TTrailer/index.md) — Base class for blocks within a file.
- [**IOFileConverter**](API/MrKWatkins.OakIO/IOFileConverter-TSource-TTarget/index.md) — Base class for format converters.
- [**IOFileConversion**](API/MrKWatkins.OakIO/IOFileConversion/index.md) — Static methods for converting between formats.

## Licencing

The project is licensed under GPL v3.0.

The ZX Spectrum file format tests use some files found in the wild:

* A snapshot of Auf Wiedersehen Monty by Gremlin Graphics. According to https://web.archive.org/web/20200228041811/http://www.worldofspectrum.org/permits/publishers.html
  Gremlin allows distribution of their games for non-profit purposes. The original snapshot was taken from https://archive.org/details/zx_Auf_Wiedersehen_Monty_1987_Gremlin_Graphics_Software;
  further snapshots in different formats were created from this.
* A TAP of Z80 Tests by Raxoft, https://github.com/raxoft/z80test, which is released under the MIT license.

If I've made a mistake with the above, please let me know.

