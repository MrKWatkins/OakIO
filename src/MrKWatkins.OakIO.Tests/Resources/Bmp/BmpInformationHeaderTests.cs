using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.Tests.Resources.Bmp;

public sealed class BmpInformationHeaderTests
{
    [TestCase(4, false, 4)]
    [TestCase(8, true, 4)]
    [TestCase(24, false, 12)]
    public void Constructor(int depth, bool topDown, int stride)
    {
        var bytes = BmpFormatTests.Create(depth, topDown);
        bytes.SetInt32(38, 1234);
        bytes.SetInt32(42, -4321);
        bytes.SetUInt32(50, 2);
        var header = BmpFormat.Instance.Read(bytes).InformationHeader;
        header.Size.Should().Equal(40u);
        header.Width.Should().Equal(3);
        header.Height.Should().Equal(topDown ? -2 : 2);
        header.IsTopDown.Should().Equal(topDown);
        header.Planes.Should().Equal((ushort)1);
        header.BitsPerPixel.Should().Equal((ushort)depth);
        header.Compression.Should().Equal(0u);
        header.ImageSize.Should().Equal((uint)(stride * 2));
        header.HorizontalPixelsPerMetre.Should().Equal(1234);
        header.VerticalPixelsPerMetre.Should().Equal(-4321);
        header.ColoursUsed.Should().Equal(depth == 24 ? 0u : 6u);
        header.ImportantColours.Should().Equal(2u);
        header.RowStride.Should().Equal(stride);
        header.Data.Should().SequenceEqual(bytes[14..54]);
    }
}