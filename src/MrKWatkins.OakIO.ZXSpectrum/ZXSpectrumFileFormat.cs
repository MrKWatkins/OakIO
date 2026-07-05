using MrKWatkins.OakIO.ZXSpectrum.Recording;
using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;
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
/// Base file format for ZX Spectrum files.
/// </summary>
public abstract class ZXSpectrumFileFormat : IOFileFormat
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
    /// All supported ZX Spectrum input recording file formats.
    /// </summary>
    public static readonly IReadOnlyList<ZXSpectrumRecordingFormat> RecordingFormats = [RzxFormat.Instance];

    /// <summary>
    /// All supported ZX Spectrum file formats.
    /// </summary>
    public static readonly IReadOnlyList<ZXSpectrumFileFormat> AllFormats =
        TapeFormats.Cast<ZXSpectrumFileFormat>().Concat(SnapshotFormats).Concat(RecordingFormats).ToArray();

    /// <summary>
    /// Initializes a new instance of the <see cref="ZXSpectrumFileFormat" /> class.
    /// </summary>
    /// <param name="name">The display name of the format.</param>
    /// <param name="fileExtension">The file extension for the format, without a leading dot.</param>
    /// <param name="fileType">The type of <see cref="ZXSpectrumFile" /> for this format.</param>
    protected ZXSpectrumFileFormat(string name, string fileExtension, Type fileType)
        : base(name, fileExtension, fileType)
    {
        if (fileType == typeof(ZXSpectrumFile) || !fileType.IsAssignableTo(typeof(ZXSpectrumFile)))
        {
            throw new ArgumentException($"The specified file type must be a subclass of {nameof(ZXSpectrumFile)}.", nameof(fileType));
        }
    }

    /// <summary>
    /// Loads a ZX Spectrum file from disk.
    /// </summary>
    /// <param name="path">The path to the file to load.</param>
    /// <returns>The file that was read.</returns>
    [Pure]
    public static ZXSpectrumFile Load([PathReference] string path) => (ZXSpectrumFile)IOFileFormat.Load(path, AllFormats);

    /// <summary>
    /// Loads a ZX Spectrum file from a stream.
    /// </summary>
    /// <param name="path">The path of the file in the stream.</param>
    /// <param name="stream">The file.</param>
    /// <returns>The file that was read.</returns>
    [MustUseReturnValue]
    public static ZXSpectrumFile Load([PathReference] string path, Stream stream) => (ZXSpectrumFile)IOFileFormat.Load(path, stream, AllFormats);

    /// <summary>
    /// Loads a ZX Spectrum file from disk asynchronously.
    /// </summary>
    /// <param name="path">The path to the file to load.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the loading.</param>
    /// <returns>The file that was read.</returns>
    [Pure]
    public static async Task<ZXSpectrumFile> LoadAsync([PathReference] string path, CancellationToken cancellationToken = default) =>
        (ZXSpectrumFile)await IOFileFormat.LoadAsync(path, AllFormats, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Loads a ZX Spectrum file from a stream asynchronously.
    /// </summary>
    /// <param name="path">The path of the file in the stream.</param>
    /// <param name="stream">The file.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the loading.</param>
    /// <returns>The file that was read.</returns>
    [MustUseReturnValue]
    public static async Task<ZXSpectrumFile> LoadAsync([PathReference] string path, Stream stream, CancellationToken cancellationToken = default) =>
        (ZXSpectrumFile)await IOFileFormat.LoadAsync(path, stream, AllFormats, cancellationToken).ConfigureAwait(false);
}