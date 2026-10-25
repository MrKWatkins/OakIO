# OakIO

[![Build Status](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml/badge.svg)](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/MrKWatkins.OakIO)](https://www.nuget.org/packages/MrKWatkins.OakIO)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MrKWatkins.OakIO)](https://www.nuget.org/packages/MrKWatkins.OakIO)

> A C# library for reading, writing, and converting between various emulator data formats.

## Overview

OakIO provides a structured object model for working with emulator and resource file formats. It supports reading and writing files, synchronously or
asynchronously, with optional compression. Once loaded their contents can be inspected and they can be converted to other formats.

## Installation

```
dotnet add package MrKWatkins.OakIO
```

ZX Spectrum formats are in `MrKWatkins.OakIO.ZXSpectrum`. ZX Spectrum Next formats, including NEX snapshots, are in
`MrKWatkins.OakIO.ZXSpectrumNext`, which references the ZX Spectrum package.

## Documentation

Full documentation can be found at https://mrkwatkins.github.io/OakIO/.

## Online Converter

An online converter using this library is available at https://mrkwatkins.github.io/OakIO/converter/.

## Use of AI

My general rule is I'll write the interesting bits and use AI for the boring bits. I wrote the bulk of the code as part of my OakEmu project before
forking it out into its own project. I have used AI to add in a few formats (SNA and NEX), some of the converters, setup for the online converter,
and help with documentation and unit tests.

## Licencing

The project is licensed under GPL v3.0.

The ZX Spectrum file format tests use some files found in the wild:

* A snapshot of Auf Wiedersehen Monty by Gremlin Graphics. According to https://web.archive.org/web/20200228041811/http://www.worldofspectrum.org/permits/publishers.html
  Gremlin allows distribution of their games for non-profit purposes. The original snapshot was taken from https://archive.org/details/zx_Auf_Wiedersehen_Monty_1987_Gremlin_Graphics_Software;
  further snapshots in different formats were created from this.
* A TAP of Z80 Tests by Raxoft, https://github.com/raxoft/z80test, which is released under the MIT license.

If I've made a mistake with the above, please let me know.

## Building and documentation

Build and Test runs on pushes and pull requests. Release runs the same validation and can publish packages, documentation, or both. Leave the version blank for
the next minor version, or enter an explicit stable version. Both publication options default to true. To update only documentation, turn **publish-packages** off
and leave **publish-docs** on. The API reference then uses the latest released source; `api-ref` can select corrected XML comments from another commit.

Handwritten documentation is in `docs/content`, with navigation and API inputs in `docs/site.json`. Run `python3 docs/preview.py` with Docker, Node and the .NET 10
SDK installed, or select **Preview documentation** in a JetBrains IDE. Open `http://localhost:5173/OakIO/`.

The shared build calls `scripts/prepare.py` to prepare the WASM workload, test and build the React converter, and stage its runtime and documentation assets.
The converter remains available on its documentation page; conversion runs entirely in the browser. Generated files stay under ignored `artifacts`, `web/dist`
and `web/dotnet` folders. For standalone web development, run `npm run serve` from `web`.

Configure NuGet trusted publishing for repository `OakIO`, workflow `release.yml`, environment `github-pages`, owner `MrKWatkins` and the OakIO packages.
Set GitHub Pages to **GitHub Actions**, and allow `main` to deploy to the `github-pages` environment. No stored NuGet API key is required.
