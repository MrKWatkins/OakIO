# Arkos AKY music

AKY is a music format from Julien Névo's [Arkos Tracker 3](https://bitbucket.org/JulienNevo/arkostracker3/src/master/). AKY stores compressed AY register changes for one or more three-channel chips.
It can be used with any machine that has an AY-3-8910 chip, including the ZX Spectrum Next with its 3 AY chips.

## File model

`MusicFile` and `MusicFormat` are resource bases. `AkyFile` retains a native `AkyHeader` and `AkyData` body. `Patterns`, their `Tracks`, and each track's `Blocks` are linked views
of those bytes. They do not store a second editable copy of the song.

The header contains a version/flags byte, the channel count, and one 32-bit chip clock per group of up to three channels. Integers use the header's declared byte order. OakIO accepts
versions zero and one, with a positive channel count and positive clocks. A final partial group still has a complete chip clock and fourteen decoded registers.

The body contains a pattern linker, track entries and compressed register blocks. Each pattern stores its duration and one track pointer per channel. A zero-duration linker
entry stores the song-loop pointer. A track entry contains a duration byte and a block pointer. A zero duration byte represents 256 frames. Register blocks start with an initial
state, followed by differential states and optional loops to non-initial states.

## Pointer origin

The header does not distinguish absolute addresses from song-relative offsets. Use the origin selected when exporting the music. `AkyFormat.Instance` uses zero, which supports
song-relative exports and files assembled at address zero. For an export assembled at $4000, read it with `new AkyFormat(0x4000)`.

Writing preserves the bytes and origin. It does not relocate pointers or change byte order. Export with the desired settings before loading, or use the matching format instance.
The complete file must fit within its origin's sixteen-bit address space.

## Decoded frames

`DecodeFrames()` enumerates one pass to the song-loop marker. Each `AkyFrame` contains fourteen chip-major registers and a per-chip `EnvelopeRetriggers` list. Register 13 must
only be written when its corresponding retrigger flag is true. Writing an unchanged shape unconditionally would restart the hardware envelope on every frame.

The decoder handles tone, noise, software and hardware envelopes, differential changes, register-block loops and all three version-one SID profiles. It produces no audio and does not schedule playback. Clock
values remain metadata; it does not transpose periods to another chip frequency. AKY also does not encode its frame rate, which must be supplied by the export/playback context.

Structural references and loop boundaries are checked when reading. Packet truncation, noise values and control loops are checked during frame enumeration. Keep this enumeration
in validation when accepting files from untrusted sources. As with other linked binary formats, arbitrary pointer targets are not proof of a meaningful musical arrangement.

## SID volume modulation

AKY SID is rapid AY volume modulation, not a Commodore SID chip data stream. It adds timing and low-shelf volume state alongside ordinary frame registers. Each frame's
`SidStates` list follows encoded channel order. A null entry means SID is disabled on that channel. An active entry has one of three native timing models.

- `AkyCpcSidState` exposes byte-sized high- and low-shelf durations.
- `AkyAmstradPlusSidState` exposes word-sized durations, also used by custom profiles.
- `AkyAtariStSidState` exposes separate MFP timer control and data bytes for each shelf. These are four individual bytes, unaffected by word byte order.

All states expose their `LowShelfVolume` and `Profile`. Durations and timer bytes remain in the export's native units; they are not converted to seconds or chip cycles.
The ordinary channel volume supplies the high-shelf volume. A renderer must apply both the register values and SID state to reproduce the sound.

Initial SID prefixes and differential enable, update and disable commands are consumed within the same frame as their following ordinary packet. Unchanged SID state persists
across differential frames, including register-block loops. Starting a new register block resets that channel's SID state. Frame snapshots are immutable and remain valid after
later state changes. SID packets require version one; a profile in the header does not itself enable SID.

`AkyHeader.SidProfile` identifies the timing encoding. Profile bits occupy bits 4–5; bit 6 is padding. This follows the exporter, whose layout differs from the older format notes.
Samples and digidrums use separately exported data and event streams in Arkos players. They are not commands embedded in an AKY register block.

## Licence

The upstream format documentation and exporter are published in the [Arkos Tracker repository under the MIT licence](https://bitbucket.org/JulienNevo/arkostracker3/src/14d975bdf0787cde4a5fa9425abe793e4a3a3536/LICENSE.txt).
No separate format licence is declared there. This concerns the upstream software and documentation; individual music files retain their own copyright and licensing.

## References

The implementation follows the [official AKY format notes](https://bitbucket.org/JulienNevo/arkostracker3/src/14d975bdf0787cde4a5fa9425abe793e4a3a3536/doc/export/AKY.md),
[exporter](https://bitbucket.org/JulienNevo/arkostracker3/src/14d975bdf0787cde4a5fa9425abe793e4a3a3536/source/export/aky/AkyExporter.cpp)
and [packet encoder](https://bitbucket.org/JulienNevo/arkostracker3/src/14d975bdf0787cde4a5fa9425abe793e4a3a3536/source/export/aky/process/RegisterBlockEncoder.cpp).
The exporter defines details missing from the prose description, including the channel-count byte, external pointer origin and the ordering of differential period bytes.

See also the [AKY player overview](https://www.julien-nevo.com/arkostracker/index.php/the-aky-player/) and [Next AY register layout](https://wiki.specnext.dev/Turbo_Sound_Next).
