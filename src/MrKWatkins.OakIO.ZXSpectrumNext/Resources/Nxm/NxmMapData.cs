using MrKWatkins.BinaryPrimitives;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

/// <summary>
/// Byte-backed NXM entries whose layout is supplied externally, not stored in an invented header.
/// </summary>
/// <remarks>
/// Layout reference: https://github.com/benbaker76/Gfx2Next/blob/e3a6abcd3bb0c721ac54d874356c868db6493a9c/src/gfx2next.c (write_map).
/// Attribute reference: https://wiki.specnext.dev/Tilemap.
/// </remarks>
public sealed class NxmMapData : IOFileComponent
{
    internal NxmMapData(byte[] bytes, NxmFormat format) : base(format.Length, bytes)
    {
        Format = format;
    }

    internal NxmMapData(TileMapData map, NxmFormat format) : this(Encode(map, format), format)
    {
    }

    /// <summary>
    /// Gets the explicit layout used to interpret these bytes.
    /// </summary>
    public NxmFormat Format { get; }

    /// <summary>
    /// Gets a derived row-major map, retaining indices and the selected attribute meanings.
    /// </summary>
    public TileMapData TileMap
    {
        get
        {
            var entries = new TileMapEntry[Format.Width * Format.Height];
            for (var y = 0; y < Format.Height; y++)
            {
                for (var x = 0; x < Format.Width; x++)
                {
                    var offset = GetOffset(x, y, Format);
                    var value = Format.BytesPerEntry == 1 ? GetByte(offset) : GetUInt16(offset);
                    entries[y * Format.Width + x] = DecodeEntry(value, Format.Encoding);
                }
            }
            return new TileMapData(Format.Width, Format.Height, entries);
        }
    }

    [Pure]
    private static int GetOffset(int x, int y, NxmFormat format) =>
        (format.Order == ResourceDataOrder.RowMajor ? y * format.Width + x : x * format.Height + y) * format.BytesPerEntry;

    [Pure]
    private static TileMapEntry DecodeEntry(ushort value, NxmEncoding encoding)
    {
        if (encoding is NxmEncoding.Index8 or NxmEncoding.Index16)
        {
            return new TileMapEntry(value);
        }
        var attributes = value >> 8;
        var extended = encoding is NxmEncoding.Tiles512 or NxmEncoding.Text512;
        var text = encoding is NxmEncoding.Text256 or NxmEncoding.Text512;
        return new TileMapEntry((value & 255) | (extended ? (attributes & 1) << 8 : 0), attributes >> (text ? 1 : 4))
        {
            MirrorX = !text && (attributes & 8) != 0,
            MirrorY = !text && (attributes & 4) != 0,
            Rotate = !text && (attributes & 2) != 0,
            Priority = !extended && (attributes & 1) != 0
        };
    }

    [Pure]
    private static byte[] Encode(TileMapData map, NxmFormat format)
    {
        ArgumentNullException.ThrowIfNull(map);
        if (map.Width != format.Width || map.Height != format.Height)
        {
            throw new ArgumentException("The map dimensions must match the selected NXM layout.", nameof(map));
        }
        var bytes = new byte[format.Length];
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var value = EncodeEntry(map.GetEntry(x, y), format.Encoding);
                var offset = GetOffset(x, y, format);
                if (format.BytesPerEntry == 1)
                {
                    bytes[offset] = (byte)value;
                }
                else
                {
                    bytes.SetUInt16(offset, value);
                }
            }
        }
        return bytes;
    }

    [Pure]
    private static ushort EncodeEntry(TileMapEntry entry, NxmEncoding encoding)
    {
        var plain = encoding is NxmEncoding.Index8 or NxmEncoding.Index16;
        var extended = encoding is NxmEncoding.Tiles512 or NxmEncoding.Text512;
        var text = encoding is NxmEncoding.Text256 or NxmEncoding.Text512;
        var maxIndex = encoding == NxmEncoding.Index16 ? 65535 : extended ? 511 : 255;
        var maxPalette = plain ? 0 : text ? 127 : 15;
        if (entry.TileIndex > maxIndex || entry.PaletteOffset > maxPalette)
        {
            throw new ArgumentException("The map index or palette offset cannot be represented in the selected encoding.", nameof(entry));
        }
        if ((plain || text) && (entry.MirrorX || entry.MirrorY || entry.Rotate))
        {
            throw new ArgumentException("The selected encoding cannot represent tile transformations.", nameof(entry));
        }
        if ((plain || extended) && entry.Priority)
        {
            throw new ArgumentException("The selected encoding cannot represent the priority flag.", nameof(entry));
        }
        if (plain)
        {
            return (ushort)entry.TileIndex;
        }
        var attributes = entry.PaletteOffset << (text ? 1 : 4);
        attributes |= (entry.MirrorX ? 8 : 0) | (entry.MirrorY ? 4 : 0) | (entry.Rotate ? 2 : 0);
        attributes |= extended ? entry.TileIndex >> 8 : entry.Priority ? 1 : 0;
        return (ushort)((entry.TileIndex & 255) | (attributes << 8));
    }
}