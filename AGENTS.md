# Agent Instructions

## Binary I/O

Use `MrKWatkins.BinaryPrimitives` extension methods for binary I/O (`stream.WriteWord()`, `stream.WriteUInt24()`, `stream.WriteUInt32()`, `array.SetWord()`, `array.SetUInt32()`, etc.). Do not create custom binary write helpers.

Prefer building binary data as byte arrays using `array.SetWord(...)`, `array.SetUInt32(...)`, etc., over writing to a `MemoryStream`. Where block classes accept byte-array constructors, pass the byte arrays directly — do not wrap them in a `MemoryStream`.

## Code Style

Do not use separator comments (e.g. `// --- Section Name ---`) to divide sections of code.

## LINQ

Use LINQ over normal for loops where possible. Prefer `foreach`, `Select`, `Where`, `FirstOrDefault`, `Sum`, `Take`, `Chunk`, etc. over index-based iteration.

Prefer lazy `IEnumerable<T>` with `yield return`/`yield break` over mutating list parameters. Use `AddRange` in the caller.
