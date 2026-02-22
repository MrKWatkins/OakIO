using MrKWatkins.OakIO.ZXSpectrum.Snapshot;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Nex;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Sna;
using MrKWatkins.OakIO.ZXSpectrum.Snapshot.Z80;
using MrKWatkins.OakIO.ZXSpectrum.Tape;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

namespace MrKWatkins.OakIO.ZXSpectrum;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ZXSpectrumFile
{
    public static readonly IReadOnlyList<ZXSpectrumTapeFormat> TapeFormats = [PzxFormat.Instance, TapFormat.Instance, TzxFormat.Instance];
    public static readonly IReadOnlyList<ZXSpectrumSnapshotFormat> SnapshotFormats = [NexFormat.Instance, SnaFormat.Instance, Z80Format.Instance];
    public static readonly IReadOnlyList<IOFileFormat> AllFormats = TapeFormats.Cast<IOFileFormat>().Concat(SnapshotFormats).ToArray();

    [Pure]
    public static IOFile Read([PathReference] string filename) => IOFile.Read(filename, AllFormats);

    [MustUseReturnValue]
    public static IOFile Read([PathReference] string filename, Stream stream) => IOFile.Read(filename, stream, AllFormats);
}