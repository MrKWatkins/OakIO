# Coding Conventions

## Lazy Enumerations
Prefer methods that return lazy `IEnumerable<T>` enumerations using `yield return`/`yield break` rather than passing in a collection to mutate. Use `AddRange` in the caller.

## Binary Primitives
Use the extension methods from `MrKWatkins.BinaryPrimitives` (e.g. `stream.WriteWord()`, `stream.WriteUInt24()`, `stream.WriteUInt32()`) for reading and writing binary data. Do not create custom binary write helpers.
