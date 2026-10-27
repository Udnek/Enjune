using Minecraft2.Block;

namespace Minecraft2.Terrain;

public interface IReadonlyChunk
{
    public int Version { get; } // changes when Position is changed
    public ChunkPos Position { get; }
    public IReadonlyChunkLayer<BlockType> BlockLayer { get; }
}