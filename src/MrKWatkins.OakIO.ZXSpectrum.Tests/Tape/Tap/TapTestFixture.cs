namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Tape.Tap;

public abstract class TapTestFixture : ZXSpectrumTestFixture
{
    [Pure]
    protected static Stream OpenZ80Test() => OpenResource(Resources.Z80TestTap);
}