using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;
using MrKWatkins.OakIO.ZXSpectrum.Tape;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum;

/// <summary>
/// Provides access to all ZX Spectrum file formats and convenience methods for reading files.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ZXSpectrumFile
{
    /// <summary>
    /// All supported ZX Spectrum tape file formats.
    /// </summary>
    public static readonly IReadOnlyList<ZXSpectrumTapeFormat> TapeFormats = [PzxFormat.Instance, TapFormat.Instance, TzxFormat.Instance];

    /// <summary>
    /// All supported ZX Spectrum snapshot file formats.
    /// </summary>
    public static readonly IReadOnlyList<ZXSpectrumSnapshotFormat> SnapshotFormats = [NexFormat.Instance, SnaFormat.Instance, Z80Format.Instance];

    /// <summary>
    /// All supported ZX Spectrum file formats.
    /// </summary>
    public static readonly IReadOnlyList<IOFileFormat> AllFormats = TapeFormats.Cast<IOFileFormat>().Concat(SnapshotFormats).ToArray();

    /// <summary>
    /// Reads a ZX Spectrum file from disk, detecting the format from the file extension.
    /// </summary>
    /// <param name="filename">The path to the file to read.</param>
    /// <returns>The file read from disk.</returns>
    [Pure]
    public static IOFile Read([PathReference] string filename) => IOFile.Read(filename, AllFormats);

    /// <summary>
    /// Reads a ZX Spectrum file from a stream, detecting the format from the filename.
    /// </summary>
    /// <param name="filename">The filename used to determine the file format.</param>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The file read from the stream.</returns>
    [MustUseReturnValue]
    public static IOFile Read([PathReference] string filename, Stream stream) => IOFile.Read(filename, stream, AllFormats);
}