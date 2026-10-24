using Minecraft2.Terrain;
using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public class BaseTerrainGenerator : ConcurrentWorker<ChunkPos, ChunkPos, Chunk>
{
    private readonly FastNoiseLite.FastNoiseLite _noise;
    private readonly App _app;
    
    public BaseTerrainGenerator(App app)
    {
        _app = app;
        _noise = new FastNoiseLite.FastNoiseLite();
    }

    protected override Chunk Work(ChunkPos chunkPos)
    {
        var chunk = new Chunk(_app.Blocks.Air);
        for (int blockX = 0; blockX < Chunk.Size.X; blockX++)
        {
            for (int blockZ = 0; blockZ < Chunk.Size.Z; blockZ++)
            {
                var y = NoiseSumAt(chunkPos.Xz, (blockX, blockZ)); // [-1; 1]
                y = (y + 1f) / 2f; // [0; 1]
                int height = Math.Clamp((int)(y * Chunk.Size.Y), 0, Chunk.Size.Y);
                height = Math.Max(height, 1);
                for (int i = 0; i < height-1; i++)
                {
                    chunk[new(blockX, i, blockZ)] = _app.Blocks.Dirt;
                }
                chunk[new(blockX, height-1, blockZ)] = _app.Blocks.Grass;
            }
        }

        return chunk;
    }

    protected float NoiseSumAt(Vector2 chunk, Vector2 block)
    {
        float x = chunk.X * Chunk.Size.X + block.X;
        float z = chunk.Y * Chunk.Size.Z + block.Y;
        //return _noise.GetNoise(x, z);
        return 0.2f * _noise.GetNoise(x, z) +
               0.3f * _noise.GetNoise(x/5f, z/5f) +
               0.5f * _noise.GetNoise(x/20f, z/20f);
    }
}