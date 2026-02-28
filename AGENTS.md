# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Project Overview

C# library providing code to read, write and convert between various emulator data formats.

## Build & Test Commands

All commands run from the root directory:

```bash
dotnet build src/OakIO.sln # Build all projects.

dotnet test --solution src/OakIO.sln  # Run all tests.
dotnet test --solution src/OakIO.sln --configuration Release -- --coverage --coverage-output coverage.xml --coverage-output-format cobertura # Run all tests with code coverage.
dotnet test --project src/MrKWatkins.OakIO.Tests  # Run a single test project.
dotnet test --project src/MrKWatkins.OakIO.Tests --filter "FullyQualifiedName~BlockTests"  # Run a single test class.

dotnet format src/OakIO.sln  # Format the source code.
```
The test project uses the NUnit runner with Microsoft Testing Platform (`EnableNUnitRunner`), so test executables can also be run directly.

### CI Build

The full CI build (`.github/workflows/build.yml`) does the following. Run these locally to replicate the build server:

```bash
# 1. Install the WASM workload (needed for MrKWatkins.OakIO.Wasm).
dotnet workload install wasm-tools

# 2. Build and test all C# projects.
dotnet build src/OakIO.sln
dotnet test --solution src/OakIO.sln

# 3. Build and test the web project.
cd web
npm ci
npm test
npm run build
```

The WASM workload is required to build the full solution. Without it, build individual projects:

```bash
dotnet build src/MrKWatkins.OakIO/MrKWatkins.OakIO.csproj
dotnet build src/MrKWatkins.OakIO.ZXSpectrum/MrKWatkins.OakIO.ZXSpectrum.csproj
```

## File Format Structure

- ** MrKWatkins.OakIO.IOFileFormat **: Base class for file formats. Contains information about the format such as name and file extension, along with methods for reading and writing files.
- ** MrKWatkins.OakIO.IOFile **: Base class for a file of a given format.
- ** MrKWatkins.OakIO.IOFileComponent **: Base class for a component of a file.
- ** MrKWatkins.OakIO.Header **: Base class for a header in a file, either for the file as a whole or a block within the file.
- ** MrKWatkins.OakIO.Trailer **: Base class for a trailer in a file, either for the file as a whole or a block within the file.
- ** MrKWatkins.OakIO.Block **: Base class for a block in a file that is composed of blocks. Each block has a header, a trailer, (which could be an **EmptyTrailer**) and data.
- ** MrKWatkins.OakIO.IOFileConverter **: Base class for converter than transforms a file of one type to another. Converts from a type are registered in the constructor for the **IOFile** of that type; they are created in the **CreateConverters** method of the **IOFileFormat**. If a converter takes parameters, e.g. converts to WAV, then a default implementation is registered.
- ** MrKWatkins.OakIO.IOFileConversion **: Static methods to convert between file formats, using the registered converters.

## Project Structure

- ** MrKWatkins.OakIO **: Main library project. Contains base classes for file formats and converters, along with Tape and Wav implementations. Tape is a generic tape file format that does not support reading/writing and is intended for use internally by emulators.
- ** MrKWatkins.OakIO.ZXSpectrum **: File formats for the ZX Spectrum. Contains tape formats that represent an actual tape, as well as snapshot formats that represent a snapshot of the a ZX Spectrum's internal state.
- ** MrKWatkins.OakIO.Commands **: Commands to be used by a CLI or web-based tool.
- ** MrKWatkins.OakIO.Tool **: A console tool for inspecting and converting files. Published as a dotnet tool.
- ** MrKWatkins.OakIO.Wasm **: A WASM library for inspecting and converting files, designed to be used by a web-based tool.

## Binary I/O

## Documentation

- Documentation is generated using MKDocs and is found in the `doc` folder.
- Documentation in `doc/docs/API` is generated from the assemblies using the sesharp tool from the root of the repository:
  - `sesharp src/MrKWatkins.OakIO/bin/Release/net10.0/MrKWatkins.OakIO.dll doc/docs/API --repository https://github.com/MrKWatkins/OakIO`
  - `sesharp src/MrKWatkins.OakIO.ZXSpectrum/bin/Release/net10.0/MrKWatkins.OakIO.ZXSpectrum.dll doc/docs/API --repository https://github.com/MrKWatkins/OakIO`
- Documentation in the root of `doc/docs` is handwritten.
- Handwritten documentation should link to the generated API documentation and Microsoft's API docs (https://learn.microsoft.com/en-us/dotnet/api/) for types, members, etc.

## Code Conventions

- Use `MrKWatkins.BinaryPrimitives` extension methods for binary I/O (`stream.WriteWord()`, `stream.WriteUInt24()`, `stream.WriteUInt32()`, `array.SetWord()`, `array.SetUInt32()`, etc.). Do not create custom binary write helpers.
- Prefer building binary data as byte arrays using `array.SetWord(...)`, `array.SetUInt32(...)`, etc., over writing to a `MemoryStream`. Where block classes accept byte-array constructors, pass the byte arrays directly — do not wrap them in a `MemoryStream`.
- Global usings configured in Directory.Build.props: `System.Diagnostics.CodeAnalysis`, `System.Diagnostics.Contracts`, `PureAttribute`, `JetBrains.Annotations`.
- Warnings are errors; CA1707 (underscores in names) is suppressed in test projects only.
- Use `[Pure]` on attributes where possible.

## Testing Conventions

- NUnit 4 with `MrKWatkins.Assertions`. (`.Should().Equal()`, `.Should().SequenceEqual()`, `.Should().Throw<>()`, `AssertThat.Invoking()`)
- Global usings for `MrKWatkins.Assertions` and `NUnit.Framework` in test projects.
- One test class per implementation class; test methods use `[TestCase]` for parameterized tests.
- Test names should match the method they're testing.
- If a method is overloaded then test names should distinguish overloads by their parameter types, separated by an underscore, e.g. `GetInt32_Span`, `GetInt32_ReadOnlySpan`.
- Extra conditions can be appended to the end of test names to distinguish them, e.g. `GetBits_InvalidRange`. The happy path should never have extra conditions.
- InternalsVisibleTo is automatically configured for test projects.
- Always try to get 100% code coverage.

## Formatting Conventions

- Always include `{` and `}` around control flow statements (e.g. `if`, `for`, `while`, etc.) even if they're single-line.
- Ensure formatting is correct by running `dotnet format src/OakIO.sln` after completing changes.
