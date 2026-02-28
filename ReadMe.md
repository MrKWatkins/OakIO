# OakIO

[![Build Status](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml/badge.svg)](https://github.com/MrKWatkins/OakIO/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/MrKWatkins.OakIO)](https://www.nuget.org/packages/MrKWatkins.OakIO)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MrKWatkins.OakIO)](https://www.nuget.org/packages/MrKWatkins.OakIO)

> A C# library for reading, writing and converting between various emulator data formats.

## Overview

OakIO provides a structured object model for working with emulator file formats. It supports reading and writing files, inspecting their contents, and converting between formats.

## Installation

```
dotnet add package MrKWatkins.OakIO
```

## Documentation

Full documentation can be found at https://mrkwatkins.github.io/OakIO/.

## Online Converter

An online converter using this library is available at https://mrkwatkins.github.io/OakIO/converter/.

## Licencing

The project is licensed under GPL v3.0.

The ZX Spectrum file format tests use some files found in the wild:

* A snapshot of Auf Wiedersehen Monty by Gremlin Graphics. According to https://web.archive.org/web/20200228041811/http://www.worldofspectrum.org/permits/publishers.html
  Gremlin allows distribution of their games for non-profit purposes. The original snapshot was taken from https://archive.org/details/zx_Auf_Wiedersehen_Monty_1987_Gremlin_Graphics_Software;
  further snapshots in different formats were created from this.
* A TAP of Z80 Tests by Raxoft, https://github.com/raxoft/z80test, which is released under the MIT license.

If I've made a mistake with the above, please let me know.