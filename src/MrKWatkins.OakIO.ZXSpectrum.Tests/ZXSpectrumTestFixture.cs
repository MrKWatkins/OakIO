using MrKWatkins.OakIO.Testing;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests;

public abstract class ZXSpectrumTestFixture
{
    [Pure]
    protected static Stream OpenResource(string resource) =>
        typeof(ZXSpectrumTestFixture).Assembly.GetManifestResourceStream(typeof(ZXSpectrumTestFixture), $"Resources.{resource}")
        ?? throw new InvalidOperationException(resource);

    [Pure]
    [MustDisposeResource]
    protected static TemporaryFile GetResourceAsTemporaryFile(string resource, string? filename = null)
    {
        using var stream = OpenResource(resource);
        return TemporaryFile.Create(stream, filename ?? resource);
    }

    protected static class Resources
    {
        // I believe Gremlin have given the rights to share and distribute their old games, according to
        // https://rk.nvg.ntnu.no/sinclair/faq/whereis1.html#CG, hence using Auf Wiedersehen Monty.
        public const string AufWiedersehenMontyZ80V1Compressed = "AufWiedersehenMonty_z80v1_Compressed.z80";
        public const string AufWiedersehenMontyZ80V1Uncompressed = "AufWiedersehenMonty_z80v1_Uncompressed.z80";
        public const string AufWiedersehenMontyZ80V2 = "AufWiedersehenMonty_z80v2.z80";
        public const string AufWiedersehenMontyZ80V2Zip = "AufWiedersehenMonty_z80v2.zip";
        public const string AufWiedersehenMontyZ80V3 = "AufWiedersehenMonty_z80v3.z80";
        public const string Z80TestTap = "Z80Testv1.2Full.tap";
        public const string UnsupportedZip = "Unsupported.zip";
    }
}