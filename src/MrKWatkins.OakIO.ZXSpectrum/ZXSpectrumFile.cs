using MrKWatkins.OakIO.ZXSpectrum.Nex;
using MrKWatkins.OakIO.ZXSpectrum.Pzx;
using MrKWatkins.OakIO.ZXSpectrum.SnaSnapshot;
using MrKWatkins.OakIO.ZXSpectrum.Tap;
using MrKWatkins.OakIO.ZXSpectrum.Tzx;
using MrKWatkins.OakIO.ZXSpectrum.Z80Snapshot;

namespace MrKWatkins.OakIO.ZXSpectrum;

public static class ZXSpectrumFile
{
    public static readonly IReadOnlyList<TapeFormat> TapeFormats = [PzxFormat.Instance, TapFormat.Instance, TzxFormat.Instance];
    public static readonly IReadOnlyList<SnapshotFormat> SnapshotFormats = [NexFormat.Instance, SnaSnapshotFormat.Instance, Z80SnapshotFormat.Instance];
    public static readonly IReadOnlyList<FileFormat> AllFormats = TapeFormats.Cast<FileFormat>().Concat(SnapshotFormats).ToArray();

    [Pure]
    public static IOFile Read([PathReference] string filename) => IOFile.Read(filename, AllFormats);

    [MustUseReturnValue]
    public static IOFile Read([PathReference] string filename, Stream stream) => IOFile.Read(filename, stream, AllFormats);
}