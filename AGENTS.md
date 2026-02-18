# Coding Conventions

## Lazy Enumerations
Prefer methods that return lazy `IEnumerable<T>` enumerations using `yield return`/`yield break` rather than passing in a collection to mutate. Use `AddRange` in the caller.

## Binary Primitives
Use the extension methods from `MrKWatkins.BinaryPrimitives` (e.g. `stream.WriteWord()`, `stream.WriteUInt24()`, `stream.WriteUInt32()`) for reading and writing binary data. Do not create custom binary write helpers.

## LINQ
Use LINQ over normal `for` loops where possible. Prefer `foreach`, `Select`, `Where`, `FirstOrDefault`, `Sum`, `Take`, `Chunk` etc. over index-based iteration.

## Comments
Do not use separator comments (e.g. `// --- Section Name ---`) to divide sections of code.
