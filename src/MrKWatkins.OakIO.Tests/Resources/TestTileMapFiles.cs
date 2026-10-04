using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

internal sealed class TestTileMapFile : TileMapFile
{
    internal TestTileMapFile(byte value) : this(new TileMapData(1, 1, [new TileMapEntry(value)])) { }
    internal TestTileMapFile(TileMapData tilemap) : this(TestTileMapFormat.Instance, tilemap) { }
    internal TestTileMapFile(TileMapFormat format, TileMapData tilemap) : base(format) => TileMap = tilemap ?? throw new ArgumentNullException(nameof(tilemap));
    public override TileMapData TileMap { get; }
}

internal sealed class TestTileMapFormat() : TileMapFormat<TestTileMapFile>("Test TileMap", "tilemap")
{
    internal static readonly TestTileMapFormat Instance = new();
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) => new TestTileMapFile((await reader.ReadToEndAsync())[0]);
    protected override ValueTask WriteAsync(TestTileMapFile file, IBinaryWriter writer) => writer.WriteAsync(new[] { (byte)file.TileMap.Entries[0].TileIndex });
}