# Home

[![Build Status](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml/badge.svg)](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml)

> A C# library for reading, writing and converting between various emulator data formats.

## Overview

OakIO provides a structured object model for working with emulator file formats. It supports reading and writing files, inspecting their contents, and converting between formats.

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

Licensed under GPL v3.0.

