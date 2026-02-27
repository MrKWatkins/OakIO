namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block containing turbo speed data with custom timing parameters.
/// </summary>
/// <param name="stream">The stream to read from.</param>
public sealed class TurboSpeedDataBlock(Stream stream) : TzxBlock<TurboSpeedDataHeader>(new TurboSpeedDataHeader(stream), stream);