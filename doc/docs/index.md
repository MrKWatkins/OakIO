# Home

[![Build Status](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml/badge.svg)](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml)

> A C# library for reading, writing and converting between various emulator data formats.

## Overview

OakIO provides a structured object model for working with emulator file formats. It supports reading and writing files, inspecting their contents, and converting between formats.

## Supported Formats

### ZX Spectrum Tape Formats

- **TAP** — Simple tape format containing raw data blocks.
- **TZX** — The standard for preserving ZX Spectrum tape recordings, supporting standard and turbo speed data, pauses, loops, groups, and archive metadata.
- **PZX** — An alternative tape format with pulse-level encoding.
- **WAV Audio** — Audio representation of tape data for playback.

### ZX Spectrum Snapshot Formats

- **Z80** — Snapshot format supporting versions 1, 2, and 3, with optional compression. Supports 48K and 128K machines.
- **SNA** — Simple snapshot format for 48K and 128K machines.
- **NEX** — Snapshot format for the ZX Spectrum Next.

## Architecture

The library is organised around a set of base classes:

- [**IOFileFormat**](API/MrKWatkins.OakIO/IOFileFormat/index.md) — Base class for file formats, providing read and write operations.
- [**IOFile**](API/MrKWatkins.OakIO/IOFile/index.md) — Base class for a file of a given format.
- [**Block**](API/MrKWatkins.OakIO/Block-THeader-TTrailer/index.md) — Base class for blocks within a file.
- [**IOFileConverter**](API/MrKWatkins.OakIO/IOFileConverter-TSource-TTarget/index.md) — Base class for format converters.
- [**IOFileConversion**](API/MrKWatkins.OakIO/IOFileConversion/index.md) — Static methods for converting between formats.

## Licencing

Licensed under GPL v3.0.
