using Minecraft2.Block;

namespace Minecraft2.Terrain;

public interface IReadonlyChunk
{
    public ChunkPos Position { get; }
    public IReadonlyChunkLayer<BlockType> BlockLayer { get; }
}