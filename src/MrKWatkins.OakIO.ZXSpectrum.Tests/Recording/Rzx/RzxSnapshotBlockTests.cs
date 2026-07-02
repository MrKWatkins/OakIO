using System.IO.Compression;
using MrKWatkins.OakIO.ZXSpectrum.Recording.Rzx;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Recording.Rzx;

public sealed class RzxSnapshotBlockTests
{
    [Test]
    public void Constructor()
    {
        var block = new RzxSnapshotBlock("Z80", [0xAA, 0xBB]);

        block.Type.Should().Equal(RzxBlockType.Snapshot);
        block.Flags.Should().Equal(RzxSnapshotFlags.None);
        block.Extension.Should().Equal("Z80");
        block.UncompressedLength.Should().Equal(2U);
        block.SnapshotData.Should().SequenceEqual(0xAA, 0xBB);
    }

    [Test]
    public void Constructor_ExplicitUncompressedLength()
    {
        var block = new RzxSnapshotBlock("SNA", [1, 2, 3], uncompressedLength: 3);

        block.Extension.Should().Equal("SNA");
        block.UncompressedLength.Should().Equal(3U);
    }

    [Test]
    public void Constructor_Bytes()
    {
        var block = new RzxSnapshotBlock("Z80", [0xAA, 0xBB]);

        // Block header: ID 0x30, block length = 5 + 14 = 19.
        block.Header.Data.Should().SequenceEqual(0x30, 0x13, 0x00, 0x00, 0x00);

        block.Data.Should().SequenceEqual(
            0x00, 0x00, 0x00, 0x00,       // Flags = None.
            (byte)'Z', (byte)'8', (byte)'0', 0x00, // Extension "Z80" zero padded to 4.
            0x02, 0x00, 0x00, 0x00,       // Uncompressed length = 2.
            0xAA, 0xBB);                   // Snapshot data.
    }

    [Test]
    public void Constructor_ExtensionExactly4Characters()
    {
        var block = new RzxSnapshotBlock("SCRN", [1]);

        // The full 4 byte field is used with no null terminator.
        block.Extension.Should().Equal("SCRN");
        block.Data.ToArray()[4..8].Should().SequenceEqual((byte)'S', (byte)'C', (byte)'R', (byte)'N');
    }

    [Test]
    public void Constructor_NullExtension_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxSnapshotBlock((string)null!, [1, 2])).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_NullData_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxSnapshotBlock("Z80", null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_Compressed_Throws()
    {
        AssertThat.Invoking(() => _ = new RzxSnapshotBlock("Z80", [1, 2], RzxSnapshotFlags.Compressed)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_TooShort_Throws()
    {
        var header = new RzxBlockHeader(RzxBlockType.Snapshot, 8);

        AssertThat.Invoking(() => _ = new RzxSnapshotBlock(header, new byte[8])).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_ExternalData_Throws()
    {
        var body = CreateBody(RzxSnapshotFlags.ExternalData, "Z80", 2, [1, 2]);
        var header = new RzxBlockHeader(RzxBlockType.Snapshot, (uint)body.Length);

        AssertThat.Invoking(() => _ = new RzxSnapshotBlock(header, body)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Read_LengthMismatch_Throws()
    {
        var body = CreateBody(RzxSnapshotFlags.None, "Z80", 5, [1, 2]);
        var header = new RzxBlockHeader(RzxBlockType.Snapshot, (uint)body.Length);

        AssertThat.Invoking(() => _ = new RzxSnapshotBlock(header, body)).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Read_Compressed()
    {
        var body = CreateBody(RzxSnapshotFlags.Compressed, "Z80", 3, Compress([1, 2, 3]));
        var header = new RzxBlockHeader(RzxBlockType.Snapshot, (uint)body.Length);

        var block = new RzxSnapshotBlock(header, body);

        block.SnapshotData.Should().SequenceEqual(1, 2, 3);
    }

    [Test]
    public void Read_CompressedLengthMismatch_Throws()
    {
        // The uncompressed length field claims 99 bytes but the data decompresses to 3.
        var body = CreateBody(RzxSnapshotFlags.Compressed, "Z80", 99, Compress([1, 2, 3]));
        var header = new RzxBlockHeader(RzxBlockType.Snapshot, (uint)body.Length);

        AssertThat.Invoking(() => _ = new RzxSnapshotBlock(header, body)).Should().Throw<InvalidDataException>();
    }

    [Pure]
    private static byte[] Compress(byte[] data)
    {
        using var stream = new MemoryStream();
        using (var zLib = new ZLibStream(stream, CompressionMode.Compress, true))
        {
            zLib.Write(data);
        }

        return stream.ToArray();
    }

    private static byte[] CreateBody(RzxSnapshotFlags flags, string extension, uint uncompressedLength, byte[] data)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true))
        {
            writer.Write((uint)flags);
            var extensionBytes = new byte[4];
            System.Text.Encoding.ASCII.GetBytes(extension).CopyTo(extensionBytes, 0);
            writer.Write(extensionBytes);
            writer.Write(uncompressedLength);
            writer.Write(data);
        }

        return stream.ToArray();
    }
}