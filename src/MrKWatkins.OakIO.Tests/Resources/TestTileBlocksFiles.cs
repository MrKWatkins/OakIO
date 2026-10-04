using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.Tests.Resources;

internal sealed class TestTileBlocksFile : TileBlocksFile
{
    internal TestTileBlocksFile(byte value) : this(new TileBlocksData(1, 1, [new TileMapEntry(value)])) { }
    internal TestTileBlocksFile(TileBlocksData blocks) : this(TestTileBlocksFormat.Instance, blocks) { }
    internal TestTileBlocksFile(TileBlocksFormat format, TileBlocksData blocks) : base(format) => Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
    public override TileBlocksData Blocks { get; }
}

internal sealed class TestTileBlocksFormat() : TileBlocksFormat<TestTileBlocksFile>("Test Blocks", "blocks")
{
    internal static readonly TestTileBlocksFormat Instance = new();
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) => new TestTileBlocksFile((await reader.ReadToEndAsync())[0]);
    protected override ValueTask WriteAsync(TestTileBlocksFile file, IBinaryWriter writer) => writer.WriteAsync(new[] { (byte)file.Blocks.Entries[0].TileIndex });
}