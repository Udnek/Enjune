using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public class BaseTerrainGenerator : ConcurrentWorker<ChunkPos, Chunk>
{
    private readonly FastNoiseLite.FastNoiseLite _noise = new();
    
    protected override Chunk Work(ChunkPos chunkPos)
    {
        var chunk = new Chunk();
        for (int blockX = 0; blockX < Chunk.Size.X; blockX++)
        {
            for (int blockZ = 0; blockZ < Chunk.Size.Z; blockZ++)
            {
                var y = _noise.GetNoise(chunkPos.X*Chunk.Size.X +blockX, chunkPos.Z*Chunk.Size.Z +blockZ); // [-1; 1]
                y = (y + 1f) / 2f; // [0; 1]
                int height = Math.Clamp((int)(y * Chunk.Size.Y), 0, Chunk.Size.Y);
                height = Math.Max(height, 1);
                for (int i = 0; i < height; i++)
                {
                    chunk[new(blockX, i, blockZ)] = true;
                }
            }
        }

        return chunk;
    }
}