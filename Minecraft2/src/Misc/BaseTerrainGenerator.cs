using Enjune.Misc;
using Enjune.Parallelism;
using Minecraft2.Block;
using Minecraft2.Terrain;
using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public class BaseTerrainGenerator : ConcurrentWorker<ChunkPos, Chunk, Chunk>
{
    private readonly FastNoiseLite.FastNoiseLite _noise;
    private readonly App _app;
    
    public BaseTerrainGenerator(App app)
    {
        _app = app;
        _noise = new FastNoiseLite.FastNoiseLite();
        _noise.SetFractalOctaves(4);
        _noise.SetFrequency(0.002f);
        _noise.SetNoiseType(FastNoiseLite.FastNoiseLite.NoiseType.OpenSimplex2);
    }

    protected override Chunk Work(Chunk chunk)
    {
        var yOffset = chunk.Position.Y * Chunk.Size.Y;
        for (int blockX = 0; blockX < Chunk.Size.X; blockX++)
        {
            for (int blockZ = 0; blockZ < Chunk.Size.Z; blockZ++)
            {
                var globalHeight = SampleHeight(chunk.Position.Xz, (blockX, blockZ)); // [-1; 1]
                globalHeight = ((globalHeight + 1f) / 2f)*256f; // [0; 256]

                var topSoil = globalHeight > 220 ? _app.Blocks.Snow : _app.Blocks.Grass;
                
                var localHeight = (int) globalHeight - yOffset;
                if (localHeight <= 0)
                    continue;
                
                for (int i = 0; i < Math.Min(Chunk.Size.Y, localHeight-2); i++)
                {
                    chunk.BlockLayer.SetByLocal((blockX, i, blockZ), _app.Blocks.Dirt);
                }

                for (int i = Math.Max(0, localHeight-2); i < Math.Min(Chunk.Size.Y, localHeight); i++)
                {
                    chunk.BlockLayer.SetByLocal((blockX, i, blockZ), topSoil);
                }
            }
        }

        return chunk;
    }

    private float SampleHeight(Vector2 chunkPos, Vector2 blockPos)
    {
        float x = chunkPos.X * Chunk.Size.X + blockPos.X;
        float z = chunkPos.Y * Chunk.Size.Z + blockPos.Y;
        return _noise.GetNoise(x, z);
        // return 0.2f * _noise.GetNoise(x, z) +
        //        0.3f * _noise.GetNoise(x/5f, z/5f) +
        //        0.5f * _noise.GetNoise(x/20f, z/20f);
    }
}