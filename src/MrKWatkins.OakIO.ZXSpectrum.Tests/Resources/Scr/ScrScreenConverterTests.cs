using MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources.Scr;

public sealed class ScrScreenConverterTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Convert(bool flashPhase)
    {
        var bytes = ScrTestData.Bytes();
        ScrScreenConverter.Convert(bytes.AsSpan(0, 6144), bytes.AsSpan(6144), flashPhase)
            .Should().SequenceEqual(ScrTestData.Pixels(bytes, flashPhase));
    }
    [TestCase(false)]
    [TestCase(true)]
    public void ConvertScalar(bool flashPhase)
    {
        var bytes = ScrTestData.Bytes();
        ScrScreenConverter.ConvertScalar(bytes.AsSpan(0, 6144), bytes.AsSpan(6144), flashPhase)
            .Should().SequenceEqual(ScrTestData.Pixels(bytes, flashPhase));
    }
    [TestCase(false)]
    [TestCase(true)]
    public void ConvertVectorized(bool flashPhase)
    {
        var bytes = ScrTestData.Bytes();
        ScrScreenConverter.ConvertVectorized(bytes.AsSpan(0, 6144), bytes.AsSpan(6144), flashPhase)
            .Should().SequenceEqual(ScrTestData.Pixels(bytes, flashPhase));
    }
    [TestCase(false)]
    [TestCase(true)]
    public void ConvertVectorized_AllAttributes(bool flashPhase)
    {
        // Every attribute paired with every possible bitmap byte, at differing locations across all thirds.
        for (var value = 0; value < 256; value++)
        {
            var bytes = new byte[6912];
            Array.Fill(bytes, (byte)value, 0, 6144);
            for (var attribute = 0; attribute < 768; attribute++)
            {
                bytes[6144 + attribute] = (byte)attribute;
            }
            var expected = ScrTestData.Pixels(bytes, flashPhase);
            ScrScreenConverter.ConvertVectorized(bytes.AsSpan(0, 6144), bytes.AsSpan(6144), flashPhase).Should().SequenceEqual(expected);
            ScrScreenConverter.ConvertScalar(bytes.AsSpan(0, 6144), bytes.AsSpan(6144), flashPhase).Should().SequenceEqual(expected);
        }
    }
}